using ModularMLAgents.Actuators;
using ModularMLAgents.Brain;
using ModularMLAgents.Compilation;
using ModularMLAgents.Editor;
using ModularMLAgents.Sensors;
using ModularMLAgents.Settings;
using ModularMLAgents.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ModularMLAgents.Models
{
    [System.Serializable]
    public class AgentGraphData : ScriptableObject
    {
        [HideInInspector]
        public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();

        [HideInInspector]
        public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();

        [HideInInspector]
        public List<AgentGraphEdgeData> Edges = new List<AgentGraphEdgeData>();

        [SubclassSelector, SerializeReference] public ITrainer Trainer = new CustomPPOTrainer();
        [SerializeField] public string PathToModel = string.Empty;

        public void Initialize(AgentGraphContext context)
        {
            PathToModel = System.IO.Path.Combine(ModularAgentsSettings.GetOrCreateSettings().DefaultModelsPath, $"{name}.py");

            var brainNode = context.CreateInstance<BrainNodeData>(typeof(BrainNodeData).Name);
            brainNode.Metadata.Position = new Rect(Vector2.zero, Vector2.zero);

            Nodes.Add(brainNode);
            AssetDatabase.AddObjectToAsset(brainNode, this);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(brainNode);
        }

        public IEnumerable<SourceNodeData> GetSources() => Nodes.OfType<SourceNodeData>();
        public IEnumerable<ConsumerNodeData> GetConsumers() => Nodes.OfType<ConsumerNodeData>();
    }

    [CustomEditor(typeof(AgentGraphData))]
    public class AgentGraphDataEditor : UnityEditor.Editor
    {
        private AgentGraphData _graphData;
        private string _behaviorName;

        private void CreateAgentModel()
        {
            var compilationContext = new CompilationContext(_graphData);
            var script = compilationContext.Compile();
            _graphData.Trainer.Hyperparameters.PathToModel = _graphData.PathToModel;
            var path = _graphData.Trainer.Hyperparameters.PathToModel;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (FileStream dataStream = new FileStream(path, FileMode.Create))
                {
                    dataStream.Write(Encoding.UTF8.GetBytes(script));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _graphData = (AgentGraphData)target;

            if (GUILayout.Button("CompileGraph"))
            {
                CreateAgentModel();
            }

            if (GUILayout.Button("Open Graph Editor"))
            {
                AgentGraph.OpenGraph(_graphData);
            }
        }
    }
}
