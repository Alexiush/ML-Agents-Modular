using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using ModularMLAgents.Compilation;
using Unity.Sentis;
using ModularMLAgents.Models;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class SensorNodeData : AgentGraphNodeData
    {
        [SerializeField] public Sensor Sensor;

        public override AgentGraphNode Load(AgentGraphContext context)
        {
            var sensorNode = new SensorNode(context, Metadata);
            sensorNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(sensorNode));

            return sensorNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            var input = compilationContext.GetInputNodes(this).First();
            var inputShape = input.GetShape(compilationContext);
            var inputReference = compilationContext.GetReference(input);

            return Sensor.Encoder.Compile(compilationContext, inputShape, inputReference);
        }

        public override TensorShape GetShape(CompilationContext compilationContext)
        {
            var input = compilationContext.GetInputNodes(this).First();
            var inputShape = input.GetShape(compilationContext);

            return Sensor.Encoder.GetShape(inputShape);
        }
    }
}