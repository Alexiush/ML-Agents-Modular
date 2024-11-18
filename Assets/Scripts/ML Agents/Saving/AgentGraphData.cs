using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
            ConfigUtilities.CreateConfig(_graphData, behaviorName, "Assets\\ML\\config\\rollerball_gen_test.yaml");
        }
    }
}
