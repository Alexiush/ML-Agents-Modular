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

            return Sensor.Encoder.Layer.Compile(
                compilationContext,
                inputShape, new List<DynamicTensorShape>(),
                GetInputSymbolicShapes(compilationContext),
                GetOutputSymbolicShapes(compilationContext),
                inputReference
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
            return Sensor.Encoder.Layer.SymbolicForwardPass(_outputSymbolicShapes);
        }

        public override List<DynamicTensorShape> GetOutputShape(IConnectionsContext connectionsContext)
        {
            var inputNodes = connectionsContext.GetInputNodes(this);

            if (inputNodes.Count == 0)
            {
                return new List<DynamicTensorShape>();
            }

            var input = connectionsContext.GetInputNodes(this).First();
            var inputShape = input.GetPartialOutputShape(connectionsContext, this);

            return Sensor.Encoder.Layer.GetShape(inputShape, new List<DynamicTensorShape>());
        }

        public override List<DynamicTensorShape> GetPartialOutputShape(IConnectionsContext connectionsContext, AgentGraphNodeData outputReceiver)
        {
            return GetOutputShape(connectionsContext);
        }
    }
}