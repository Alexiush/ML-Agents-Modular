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
}

[CustomEditor(typeof(AgentGraphData))] // Replace MyGraphAsset with your actual graph asset class
public class AgentGraphDataEditor : Editor
{
    private AgentGraphData _graphData;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        _graphData = (AgentGraphData)target;

        if (GUILayout.Button("Open Graph Editor"))
        {
            AgentGraph.OpenGraph(_graphData);
        }
    }
}
