using ModularMLAgents.Actuators;
using MLAgents.Configuration;
using ModularMLAgents.Models;
using ModularMLAgents.Sensors;
using ModularMLAgents.Trainers;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;

namespace ModularMLAgents.Components
{
    // Agent contains a lot of things that are of no use there
    // Eventually it can be switched from being wrapper to being implementation of same interfaces

    public abstract class ModularAgent : Agent
    {
        // Does not need to overwrite Await
        // Does not need to overwrite OnEnable

        public AgentGraphData GraphData;
        public BehaviorConfig BehaviorConfig;

        [System.Serializable]
        public class SourceProviderEntry
        {
            public string Name;
            public SourceProvider SourceProvider;
        }

        [System.Serializable]
        public class ConsumerProviderEntry
        {
            public string Name;
            public ConsumerProvider ConsumerProvider;
        }

        [HideInInspector]
        public List<SourceProviderEntry> SourceProviders = new();
        private List<SourceProviderEntry> _sourceProvidersCache = new();

        [HideInInspector]
        public List<ConsumerProviderEntry> ConsumerProviders = new();
        private List<ConsumerProviderEntry> _consumerProvidersCache = new();

        public void UpdateMapping()
        {
            SourceProviders = GraphData.GetSources()
                .Select(s =>
                {
                    return SourceProviders.FindIndex(sp => sp.Name == s.name) switch
                    {
                        -1 => new SourceProviderEntry { Name = s.name, SourceProvider = null },
                        int i => SourceProviders[i],
                    };
                })
                .ToList();

            ConsumerProviders = GraphData.GetConsumers()
                .Select(c =>
                {
                    return ConsumerProviders.FindIndex(cp => cp.Name == c.name) switch
                    {
                        -1 => new ConsumerProviderEntry { Name = c.name, ConsumerProvider = null },
                        int i => ConsumerProviders[i],
                    };
                })
                .ToList();

#if UNITY_EDITOR
            var sourcesMatch = _sourceProvidersCache.Count == SourceProviders.Count
                && _sourceProvidersCache.All(e => SourceProviders.Any(s => s.Name == e.Name && s.SourceProvider == e.SourceProvider));
            var consumersMatch = _consumerProvidersCache.Count == ConsumerProviders.Count
                && _consumerProvidersCache.All(e => ConsumerProviders.Any(s => s.Name == e.Name && s.ConsumerProvider == e.ConsumerProvider));

            if (!sourcesMatch || !consumersMatch)
            {
                EditorUtility.SetDirty(this);
                _sourceProvidersCache = SourceProviders
                    .Select(e => new SourceProviderEntry { Name = e.Name, SourceProvider = e.SourceProvider })
                    .ToList();
                _consumerProvidersCache = ConsumerProviders
                    .Select(e => new ConsumerProviderEntry { Name = e.Name, ConsumerProvider = e.ConsumerProvider })
                    .ToList();
            }
#endif
        }

        private void GenerateMappingFile()
        {
            var path = (BehaviorConfig.Behavior.Trainer as ICustomTrainer).CustomHyperparameters.PathToMapping;
            var entries = new List<(string, string)>();

            SourceProviders
                .ForEach(e => entries.Add((e.Name, e.SourceProvider.SourcesCount.ToString())));

            ConsumerProviders
                .ForEach(e => entries.Add((e.Name, e.ConsumerProvider.ConsumersCount.ToString())));

            MappingGenerator.GenerateMappingFile(path, entries);
        }

        public override void Initialize()
        {
            GenerateMappingFile();
            base.Initialize();
        }

        // Does not need to overwrite OnDisable

        // Does not need to overwrite CollectObservations, but also does not really need that feature too

        // Does not need to overwrite WriteDiscreteActionMask (for now, in future actuators may change that behavior)

        // Received actions now go to actuators (not sure if that need to be overwritten, does not seem to be for Match3)

        // Does not need to overwrite OnEpisodeBegin
    }

    [CustomEditor(typeof(ModularAgent), true)]
    public class ModularAgentEditor : UnityEditor.Editor
    {
        private ModularAgent _modularAgent;
        private bool _showSources;
        private bool _showConsumers;

        private AgentGraphData _graphData;

        private List<string> ValidateSources()
        {
            List<string> sourcesValidationMessages = new List<string>();

            if (_modularAgent.SourceProviders.Any(p => p.SourceProvider is null))
            {
                sourcesValidationMessages.Add("Not all sources are registered");
            }

            foreach (var item in _modularAgent.SourceProviders.Select((source, i) => new { i, source }))
            {
                if (item.source.SourceProvider is null)
                {
                    continue;
                }

                var providerTensorShape = ShapeUtilities.ObservationsAsTensor(item.source.SourceProvider.ObservationSpec);
                if (providerTensorShape != _graphData.GetSources().ElementAt(item.i).Source.OutputShape.AsTensorShape())
                {
                    sourcesValidationMessages.Add($"{item.source.Name}: Provider's spec does not match");
                }
            }

            return sourcesValidationMessages;
        }

        private List<string> ValidateConsumers()
        {
            List<string> consumersValidationMessages = new List<string>();

            if (_modularAgent.ConsumerProviders.Any(p => p.ConsumerProvider is null))
            {
                consumersValidationMessages.Add("Not all consumers are registered");
            }

            foreach (var item in _modularAgent.ConsumerProviders.Select((consumer, i) => new { i, consumer }))
            {
                if (item.consumer.ConsumerProvider is null)
                {
                    continue;
                }

                var consumerSpec = item.consumer.ConsumerProvider.ActionSpec;
                var graphSpec = _graphData.GetConsumers().ElementAt(item.i).Consumer.ActionModel.ActionSpec;
                if (!ShapeUtilities.CompareActionSpecs(consumerSpec, graphSpec))
                {
                    consumersValidationMessages.Add($"{item.consumer.Name}: Provider's spec does not match");
                }
            }

            return consumersValidationMessages;
        }

        public override void OnInspectorGUI()
        {
            _modularAgent = (ModularAgent)target;
            _graphData = _modularAgent.GraphData;

            base.OnInspectorGUI();
            if (_modularAgent.GraphData != null)
            {
                _graphData = _modularAgent.GraphData;
                _modularAgent.UpdateMapping();
            }

            List<string> sourcesValidationMessages = ValidateSources();
            if (sourcesValidationMessages.Count > 0)
            {
                EditorGUILayout.HelpBox($"Errors with sources:\n {string.Join("\n", sourcesValidationMessages)}", MessageType.Error);
            }

            _showSources = EditorGUILayout.Foldout(_showSources, "Sources");
            if (_showSources)
            {
                var sourceEntries = _modularAgent.SourceProviders.ToList();
                foreach (var item in sourceEntries.Select((entry, i) => new { i, entry }))
                {
                    _modularAgent.SourceProviders[item.i].SourceProvider =
                        EditorGUILayout.ObjectField(item.entry.Name, item.entry.SourceProvider, typeof(SourceProvider)) as SourceProvider;
                }
            }

            List<string> consumersValidationMessages = ValidateConsumers();
            if (consumersValidationMessages.Count > 0)
            {
                EditorGUILayout.HelpBox($"Errors with consumers:\n {string.Join("\n", consumersValidationMessages)}", MessageType.Error);
            }

            _showConsumers = EditorGUILayout.Foldout(_showConsumers, "Consumers");
            if (_showConsumers)
            {
                var consumerEntries = _modularAgent.ConsumerProviders.ToList();
                foreach (var item in consumerEntries.Select((entry, i) => new { i, entry }))
                {
                    _modularAgent.ConsumerProviders[item.i].ConsumerProvider =
                        EditorGUILayout.ObjectField(item.entry.Name, item.entry.ConsumerProvider, typeof(ConsumerProvider)) as ConsumerProvider;
                }
            }
        }
    }
}
