using System.Collections.Generic;

namespace ModularMLAgents.Models
{
    public interface IConnectionsContext
    {
        public List<AgentGraphNodeData> GetInputNodes(AgentGraphNodeData node);
        public List<string> GetInputs(AgentGraphNodeData node);
        public List<AgentGraphNodeData> GetOutputNodes(AgentGraphNodeData node);
        public List<string> GetOutputs(AgentGraphNodeData node);
    }
}
