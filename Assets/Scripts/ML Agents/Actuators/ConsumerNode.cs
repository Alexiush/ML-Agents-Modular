using ModularMLAgents.Layers;
using ModularMLAgents.Utilities;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class Consumer
    {
        // Consumer represents an action buffer where the decoder output will be written

        public ActionModel ActionModel;
    }

    [NodePath("Consumer")]
    public class ConsumerNode : AgentGraphNodeBase<ConsumerNodeData>
    {
        private Consumer Consumer => (RuntimeData as ConsumerNodeData).Consumer;

        public ConsumerNode(AgentGraphContext context, ConsumerNodeData data = null) : base(context, data)
        {
            if (data is null)
            {
                Data = context.CreateInstance<ConsumerNodeData>(GetType().Name);

                Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Tensor));
                inputPort.name = "Input signal";
            }
            else
            {
                Metadata = data.Metadata;
                viewDataKey = Metadata.GUID;

                Data = data;
            }

            RuntimeData = context.CreateInstance<ConsumerNodeData>(Data.name);
            EditorUtility.CopySerialized(Data, RuntimeData);
        }

        public override ValidationReport Validate(ValidationReport validationReport)
        {
            // Consumer should have an input
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                validationReport.Errors.Add("Consumer requires an input");
            }

            // Consumer's input should have correct shape for its action model (for now it just should be flat)
            var inputShape = GetInputShape();
            bool correctInputShape = inputShape.Count == 1 && inputShape[0].rank == 1;
            if (inputShape.Count > 0 && !correctInputShape)
            {
                validationReport.Errors.Add("Consumer's input should be flattened'");
            }

            return base.Validate(validationReport);
        }
    }
}
