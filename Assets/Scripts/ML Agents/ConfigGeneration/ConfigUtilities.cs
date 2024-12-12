using ModularMLAgents.Models;
using ModularMLAgents.Trainers;
using ModularMLAgents.Utilities;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Serialization;

namespace ModularMLAgents.Configuration
{
    public static class ConfigUtilities
    {
        public static Behavior CreateBehavior(AgentGraphData graphData, string behaviorId)
        {
            var behavior = new Behavior();

            behavior.BehaviorId = behaviorId;
            behavior.TrainerType = graphData.Trainer.TrainerType;
            behavior.Hyperparameters = graphData.Trainer.Hyperparameters;

            return behavior;
        }

        public static void CreateConfig(Config configuration, string path)
        {
            var options = YamlSerializerOptions.Standard;
            options.NamingConvention = NamingConvention.SnakeCase;

            options.Resolver = CompositeResolver.Create(
                new IYamlFormatter[]
                {
                    new ConfigurationFormatter(),
                    new HyperparametersFormatter(),
                    new RewardSignalsFormatter(),
                    new BehaviorFormatter(),
                    new EnvironmentParametersFormatter(),
                    new NullWhenEmptyStringFormatter(),
                    new SamplerFormatter(),
                    new LessonValueFormatter()
                },
                new IYamlFormatterResolver[] {
                    StandardResolver.Instance,
                }
            );

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
