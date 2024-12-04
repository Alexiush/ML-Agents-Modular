using System.Collections.Generic;
using UnityEngine;

namespace ModularMLAgents.Models
{
    [System.Serializable]
    public class AgentGraphGroupData : ScriptableObject
    {
        [SerializeField] public AgentGraphElementMetadata Metadata;
        [SerializeField] public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
        [SerializeField] public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();

        public AgentGraphGroup Load(AgentGraphContext context)
        {
            var agentGraphGroup = new AgentGraphGroup(context, Metadata);
            agentGraphGroup.SetPosition(Metadata.Position);
            agentGraphGroup.title = Metadata.Asset.name;

            foreach (var node in Nodes)
            {
                agentGraphGroup.AddElement(node.Load(context));
            }

            foreach (var group in Groups)
            {
                agentGraphGroup.AddElement(group.Load(context));
            }

            return agentGraphGroup;
        }
    }
}
