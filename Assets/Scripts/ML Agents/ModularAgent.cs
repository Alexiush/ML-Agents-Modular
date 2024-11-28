using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using UnityEditor;
using System.Linq;
using ModularMLAgents.Sensors;
using ModularMLAgents.Actuators;
using ModularMLAgents.Models;

namespace ModularMLAgents
{
    // Agent contains a lot of things that are of no use there
    // Eventually it can be switched from being wrapper to being implementation of same interfaces

    public abstract class ModularAgent : Agent
    {
        // Does not need to overwrite Await
        // Does not need to overwrite OnEnable

        public AgentGraphData GraphData;

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

        // These dictionaries should be backed by lists to be serializable
        [HideInInspector]
        public List<SourceProviderEntry> SourceProviders;
        [HideInInspector]
        public List<ConsumerProviderEntry> ConsumerProviders;

        public void ResetMapping()
        {
            Debug.Log("Reset mapping");

            // Only if graph data had changed
            SourceProviders = GraphData.GetSources()
                .Select(s => new SourceProviderEntry { Name = s.name, SourceProvider = null })
                .ToList();

            ConsumerProviders = GraphData.GetConsumers()
                .Select(c => new ConsumerProviderEntry { Name = c.name, ConsumerProvider = null })
                .ToList();
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

        public override void OnInspectorGUI()
        {
            _modularAgent = (ModularAgent)target;
            _graphData = _modularAgent.GraphData;

            base.OnInspectorGUI();
            if (_graphData != null && _graphData != _modularAgent.GraphData)
            {
                Debug.Log(_graphData);
                Debug.Log(_modularAgent.GraphData);

                _graphData = _modularAgent.GraphData;
                _modularAgent.ResetMapping();
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

            EditorUtility.SetDirty(_modularAgent);
        }
    }
}
