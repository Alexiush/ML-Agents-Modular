using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class SensorNodeData : AgentGraphNodeData
    {
        [SerializeField]
        [ValidationObserved]
        public Sensor Sensor;

        public override IAgentGraphNode Load(AgentGraphContext context)
        {
            var sensorNode = new SensorNode(context, this);
            sensorNode.SetPosition(Metadata.Position);
            Ports.ForEach(p => p.Instantiate(sensorNode));

            return sensorNode;
        }

        public override string GetExpressionBody(CompilationContext compilationContext)
        {
            var input = compilationContext.GetInputNodes(this).First();
            var inputShape = input.GetPartialOutputShape(compilationContext, this);
            var inputReference = compilationContext.GetReference(input);

            return Sensor.Encoder.Layer.Compile(compilationContext, inputShape, new List<TensorShape>(), inputReference);
        }

        public override string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return "";
        }

        public override List<TensorShape> GetOutputShape(IConnectionsContext compilationContext)
        {
            var inputNodes = compilationContext.GetInputNodes(this);

            if (inputNodes.Count == 0)
            {
                return new List<TensorShape>();
            }

            var input = compilationContext.GetInputNodes(this).First();
            var inputShape = input.GetPartialOutputShape(compilationContext, this);

            return Sensor.Encoder.Layer.GetShape(inputShape, new List<TensorShape>());
        }

        public override List<TensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(compilationContext);
        }
    }
}