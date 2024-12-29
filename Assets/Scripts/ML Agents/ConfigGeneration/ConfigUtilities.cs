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
        public static void BindBehaviorToAgentGraph(Behavior behavior, AgentGraphData graphData)
        {
            if (behavior.Trainer is ICustomTrainer customTrainer)
            {
                customTrainer.CustomHyperparameters.PathToModel = $"{System.IO.Path.Combine(graphData.ModelDirectory, graphData.name)}.py";
                customTrainer.CustomHyperparameters.PathToMapping = $"{System.IO.Path.Combine(graphData.ModelDirectory, behavior.BehaviorId)}.py";
            }
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
                    new LessonValueFormatter(),
                    new BehaviorNameFormatter()
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
