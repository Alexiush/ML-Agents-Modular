using System.Linq;
using UnityEngine;
using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using Unity.Sentis;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class ActuatorNodeData : AgentGraphNodeData
    {
        [SerializeField] public Actuator Actuator;

        public override AgentGraphNode Load(AgentGraphContext context)
        {
            var actuatorNode = new ActuatorNode(context, Metadata);
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

            return Actuator.Decoder.Compile(compilationContext, input);
        }

        public override TensorShape GetShape(CompilationContext compilationContext)
        {
            // Input shape of an actuator is what it requests, not what brain has "hidden"
            return Actuator.Decoder.GetShape(Actuator.InputShape.ToShape());
        }
    }
}
