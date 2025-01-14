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
    public class ActuatorNodeData : AgentGraphNodeData, IShapeRequestor
    {
        [SerializeField]
        [ValidationObserved]
        public Actuator Actuator;

        public override IAgentGraphNode Load(AgentGraphContext context)
        {
            var actuatorNode = new ActuatorNode(context, this);
            actuatorNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(actuatorNode));

            return actuatorNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            var inputNode = compilationContext.GetInputNodes(this).First();
            var id = compilationContext.GetOutputNodes(inputNode).IndexOf(this);
            var input = $"{compilationContext.GetReference(inputNode)}{inputNode.GetAccessor(compilationContext, this)}";

            return Actuator.Decoder.Layer.Compile(
                compilationContext,
                new List<DynamicTensorShape>() { new DynamicTensorShape(Actuator.InputShape.AsTensorShape()) },
                new List<DynamicTensorShape>() { },
                GetInputSymbolicShapes(compilationContext),
                GetOutputSymbolicShapes(compilationContext),
                input
            );
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public override List<SymbolicTensorDim> GetInputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            return _inputSymbolicShapes;
        }

        public override List<SymbolicTensorDim> GetOutputSymbolicShapes(IConnectionsContext connectionsContext)
        {
            return Actuator.Decoder.Layer.SymbolicForwardPass(_outputSymbolicShapes);
        }

        public List<DynamicTensorShape> GetRequestedShape()
        {
            return new List<DynamicTensorShape>() { new DynamicTensorShape(Actuator.InputShape.AsTensorShape()) };
        }

        public override List<DynamicTensorShape> GetOutputShape(IConnectionsContext connectionsContext)
        {
            // Input shape of an actuator is what it requests, not what brain has "hidden"
            return Actuator.Decoder.Layer.GetShape(
                new List<DynamicTensorShape>() { new DynamicTensorShape(Actuator.InputShape.AsTensorShape()) },
                new List<DynamicTensorShape>() { }
            );
        }

        public override List<DynamicTensorShape> GetPartialOutputShape(IConnectionsContext connectionsContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(connectionsContext);
        }
    }
}
