using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class SourceNodeData : AgentGraphNodeData
    {
        [SerializeField]
        public Source Source;

        public override IAgentGraphNode Load(AgentGraphContext context)
        {
            var sourceNode = new SourceNode(context, this);
            sourceNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(sourceNode));

            return sourceNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            // Input variable is predefined
            var input = $"input_tensor[{compilationContext.GetSourceNumber(this)}]";

            // Source should be taking its tensor by index
            return input;
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public override List<TensorShape> GetOutputShape(IConnectionsContext compilationContext)
        {
            return new List<TensorShape>() { Source.OutputShape.AsTensorShape() };
        }

        public override List<TensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(compilationContext);
        }
    }
}
