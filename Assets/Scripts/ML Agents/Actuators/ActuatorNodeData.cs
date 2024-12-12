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
            // Actuator gets his number in brain outputs and passes this value to the decoder
            var brain = compilationContext.GetInputNodes(this).First();
            var id = compilationContext.GetOutputNodes(brain).IndexOf(this);
            var input = $"{compilationContext.GetReference(brain)}[{id}]";

            return Actuator.Decoder.Layer.Compile(
                compilationContext,
                new List<TensorShape>() { Actuator.InputShape.AsTensorShape() },
                new List<TensorShape>() { },
                input
            );
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public List<TensorShape> GetRequestedShape()
        {
            return new List<TensorShape>() { Actuator.InputShape.AsTensorShape() };
        }

        public override List<TensorShape> GetOutputShape(IConnectionsContext compilationContext)
        {
            // Input shape of an actuator is what it requests, not what brain has "hidden"
            return Actuator.Decoder.Layer.GetShape(
                new List<TensorShape>() { Actuator.InputShape.AsTensorShape() },
                new List<TensorShape>() { }
            );
        }

        public override List<TensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(compilationContext);
        }
    }
}
