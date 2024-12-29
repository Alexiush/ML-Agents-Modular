using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using System.Collections.Generic;
using System.Linq;
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
            var size = GetOutputSymbolicShapes(compilationContext).First().Compile();
            var input = $"torch.cat([t.unsqueeze(1) for t in input_tensor[_offset:_offset+{size}]], dim=1); _offset += {size}";

            // Source should be taking its tensor by index
            return input;
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public override List<SymbolicTensorDim> GetInputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            throw new System.NotImplementedException();
        }

        public override List<SymbolicTensorDim> GetOutputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            var literal = new LiteralSymbolicTensorDim(name);
            return new List<SymbolicTensorDim> { literal };
        }

        public override List<DynamicTensorShape> GetOutputShape(IConnectionsContext connectionsContext)
        {
            return new List<DynamicTensorShape>() { new DynamicTensorShape(Source.OutputShape.AsTensorShape()) };
        }

        public override List<DynamicTensorShape> GetPartialOutputShape(IConnectionsContext connectionsContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(connectionsContext);
        }
    }
}
