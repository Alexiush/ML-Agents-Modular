using ModularMLAgents.Utilities;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class Source
    {
        // As the agent works asynchronously it can't use sensor itself to consume input
        // Input source will be an actual input that will eventually be copied onto the sensor
        [ValidationObserved]
        public Schema OutputShape;
    }

    [NodePath("Source")]
    public class SourceNode : AgentGraphNodeBase<SourceNodeData>
    {
        private Source Source => (RuntimeData as SourceNodeData).Source;

        public SourceNode(AgentGraphContext context, SourceNodeData data) : base(context, data)
        {
            if (data == null)
            {
                Data = context.CreateInstance<SourceNodeData>(GetType().Name);

                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
                outputPort.name = "Output signal";
            }
            else
            {
                Metadata = data.Metadata;
                viewDataKey = Metadata.GUID;

                Data = data;
            }

            RuntimeData = context.CreateInstance<SourceNodeData>(Data.name);
            EditorUtility.CopySerialized(Data, RuntimeData);
        }

        public override ValidationReport Validate(ValidationReport validationReport)
        {
            // Source should have output
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                validationReport.Errors.Add("Source should have an output");
            }

            return base.Validate(validationReport);
        }
    }
}