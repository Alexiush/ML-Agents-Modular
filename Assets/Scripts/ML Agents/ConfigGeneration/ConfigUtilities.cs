using System;
using System.IO;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Serialization;
using System.Text;
using VYaml.Emitter;
using VYaml.Parser;
using ModularMLAgents.Trainers;
using ModularMLAgents.Models;

namespace ModularMLAgents.Configuration
{
    public static class ConfigUtilities
    {
        public class ConfigurationFormatter : IYamlFormatter<Configuration>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, Configuration value, YamlSerializationContext context)
            {
                if (value is null)
                {
                    emitter.WriteNull();
                    return;
                }

                emitter.BeginMapping();
                {
                    emitter.WriteString("behaviors");
                    var formatter = context.Resolver.GetFormatterWithVerify<Behavior>();

                    foreach (var behavior in value.Behaviors)
                    {
                        emitter.BeginMapping();
                        {
                            emitter.WriteString(behavior.BehaviorId);
                            formatter.Serialize(ref emitter, behavior, context);
                        }
                    }
                }
            }

            public Configuration Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                throw new NotImplementedException("This formatter is write-only");
            }
        }

        public class HyperparametersFormatter : IYamlFormatter<Hyperparameters>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, Hyperparameters value, YamlSerializationContext context)
            {
                value.Serialize(ref emitter, context);
            }

            public Hyperparameters Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                throw new NotImplementedException("This formatter is write-only");
            }
        }

        public static void CreateConfig(AgentGraphData graphData, string behaviorId, string path)
        {
            var behavior = new Behavior();

            behavior.BehaviorId = behaviorId;
            behavior.TrainerType = graphData.Trainer.TrainerType;
            behavior.Hyperparameters = graphData.Trainer.Hyperparameters;

            var options = YamlSerializerOptions.Standard;
            options.NamingConvention = NamingConvention.SnakeCase;

            options.Resolver = CompositeResolver.Create(
                new IYamlFormatter[]
                {
                new ConfigurationFormatter(),
                new HyperparametersFormatter()
                },
                new IYamlFormatterResolver[] {
                StandardResolver.Instance,
                }
            );

            var configuration = new Configuration();
            configuration.Behaviors.Add(behavior);
            var config = YamlSerializer.SerializeToString(configuration, options);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (FileStream dataStream = new FileStream(path, FileMode.Create))
                {
                    dataStream.Write(Encoding.UTF8.GetBytes(config));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
