using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.MLAgents;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AgentGraphData : ScriptableObject 
{
    [SerializeField] public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
    [SerializeField] public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();
    [SerializeField] public List<AgentGraphEdgeData> Edges = new List<AgentGraphEdgeData>();

    [SubclassSelector, SerializeReference] public ITrainer Trainer = new CustomPPOTrainer();

    public void Initialize()
    {
        var brainNode = ScriptableObject.CreateInstance<BrainNodeData>();
        brainNode.Metadata.Position = new Rect(Vector2.zero, Vector2.zero);

        Nodes.Add(brainNode);
        AssetDatabase.AddObjectToAsset(brainNode, this);
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(brainNode);
    }

    public IEnumerable<SourceNodeData> GetSources() => Nodes
        .Where(n => n is SourceNodeData)
        .Cast<SourceNodeData>();

    public IEnumerable<ConsumerNodeData> GetConsumers() => Nodes
        .Where(n => n is ConsumerNodeData)
        .Cast<ConsumerNodeData>();
}

[CustomEditor(typeof(AgentGraphData))] // Replace MyGraphAsset with your actual graph asset class
public class AgentGraphDataEditor : Editor
{
    private AgentGraphData _graphData;
    private string _behaviorName;

    private void CreateAgentModel()
    {
        var compilationContext = new CompilationContext(_graphData);
        var script = compilationContext.Compile();
        var path = (_graphData.Trainer.Hyperparameters as CustomPPOTrainerHyperparameters).PathToModel;

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

        if (GUILayout.Button("Open Graph Editor"))
        {
            AgentGraph.OpenGraph(_graphData);
        }

        var behaviorName = GUILayout.TextField(_behaviorName);
        _behaviorName = behaviorName;

        if (GUILayout.Button("CompileGraph"))
        {
            CreateAgentModel();
            ConfigUtilities.CreateConfig(_graphData, behaviorName, "Assets\\ML\\config\\rollerball_gen_test.yaml");
        }
    }
}
