using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AgentGraphData : ScriptableObject 
{
    [SerializeField] public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
    [SerializeField] public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();
    [SerializeField] public List<AgentGraphEdgeData> Edges = new List<AgentGraphEdgeData>();

    [SubclassSelector, SerializeReference] public ITrainer Trainer;
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
