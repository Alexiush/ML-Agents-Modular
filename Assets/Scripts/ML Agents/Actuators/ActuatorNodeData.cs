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

        public override AgentGraphNode Load()
        {
            var actuatorNode = new ActuatorNode(Metadata);
            actuatorNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(actuatorNode));

            return actuatorNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            // For now: get inputs
            var input = compilationContext.GetInputs(this).First();
            // No decoders for now
            return Actuator.Decoder.Compile(compilationContext, input);
        }

        public override TensorShape GetShape(CompilationContext compilationContext)
        {
            var inputShape = compilationContext
                .GetInputNodes(this)
                .First()
                .GetShape(compilationContext);

            return Actuator.Decoder.GetShape(inputShape);
        }
    }
}
