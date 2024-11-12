using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentGraphGroupData : ScriptableObject
{
    [SerializeField] public AgentGraphElementMetadata Metadata;
    [SerializeField] public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
    [SerializeField] public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();

    public AgentGraphGroup Load()
    {
        var agentGraphGroup = new AgentGraphGroup(Metadata);
        agentGraphGroup.SetPosition(Metadata.Position);

        foreach (var node in Nodes)
        {
            agentGraphGroup.AddElement(node.Load());
        }

        foreach (var group in Groups)
        {
            agentGraphGroup.AddElement(group.Load());
        }

        return agentGraphGroup;
    }
}
