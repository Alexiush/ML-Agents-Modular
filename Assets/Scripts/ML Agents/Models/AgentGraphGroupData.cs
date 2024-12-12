using System.Collections.Generic;
using UnityEngine;

namespace ModularMLAgents.Models
{
    [System.Serializable]
    public class AgentGraphGroupData : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public AgentGraphElementMetadata Metadata;
        [SerializeField]
        [HideInInspector]
        public List<AgentGraphNodeData> Nodes = new List<AgentGraphNodeData>();
        [SerializeField]
        [HideInInspector]
        public List<AgentGraphGroupData> Groups = new List<AgentGraphGroupData>();

        public AgentGraphGroup Load(AgentGraphContext context)
        {
            var agentGraphGroup = new AgentGraphGroup(context, this);
            agentGraphGroup.SetPosition(Metadata.Position);
            agentGraphGroup.title = this.name;

            foreach (var node in Nodes)
            {
                agentGraphGroup.AddElement(node.Load(context).Node);
            }

            foreach (var group in Groups)
            {
                agentGraphGroup.AddElement(group.Load(context));
            }

            return agentGraphGroup;
        }
    }
}
