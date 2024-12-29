using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class ConsumerNodeData : AgentGraphNodeData
    {
        [SerializeField]
        [ValidationObserved]
        public Consumer Consumer;

        public override IAgentGraphNode Load(AgentGraphContext context)
        {
            var consumerNode = new ConsumerNode(context, this);
            consumerNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(consumerNode));

            return consumerNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            // For now: get inputs
            var inputs = compilationContext.GetInputNodes(this);
            // Consumer has the only input
            var input = inputs.First();

            compilationContext.RegisterEndpoint(this);
            compilationContext.RegisterActionModel(
                $"ActionModel({input.GetPartialOutputShape(compilationContext, this)[0]}, ActionSpec({Consumer.ActionSpec.NumContinuousActions}, ({string.Join(", ", Consumer.ActionSpec.BranchSizes)})))",
                GetInputSymbolicShapes(compilationContext).First().Compile()
            );

            // Consumer just aliases inputs and used to create big output tensor
            return compilationContext.GetReference(input);
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public override List<SymbolicTensorDim> GetInputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            var literal = new LiteralSymbolicTensorDim(name);
            return new List<SymbolicTensorDim> { literal };
        }

        public override List<SymbolicTensorDim> GetOutputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            throw new System.NotImplementedException();
        }

        public override List<DynamicTensorShape> GetOutputShape(IConnectionsContext connectionsContext)
        {
            throw new System.NotImplementedException();
        }

        public override List<DynamicTensorShape> GetPartialOutputShape(IConnectionsContext connectionsContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(connectionsContext);
        }
    }
}
