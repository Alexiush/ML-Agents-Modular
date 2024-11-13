using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentGraphData : ScriptableObject 
{
    [SerializeField] public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
    [SerializeField] public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();
    [SerializeField] public List<AgentGraphEdgeData> Edges = new List<AgentGraphEdgeData>();
}
