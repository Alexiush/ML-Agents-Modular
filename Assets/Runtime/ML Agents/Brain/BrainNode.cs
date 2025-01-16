using ModularMLAgents.Models;
using ModularMLAgents.Utilities;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace ModularMLAgents.Brain
{
    [NodePath("Brain")]
    public class BrainNode : AgentGraphNodeBase<BrainNodeData>
    {
        private Brain Brain => (RuntimeData as BrainNodeData).Brain;

        public BrainNode(AgentGraphContext context, BrainNodeData data = null) : base(context, data)
        {
            if (data == null)
            {
                Data = context.CreateInstance<BrainNodeData>(GetType().Name);

                Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Tensor));
                inputPort.name = "Input signal";

                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
                outputPort.name = "Output signal";
            }
            else
            {
                Metadata = data.Metadata;
                viewDataKey = Metadata.GUID;

                Data = data;
            }

            RuntimeData = context.CreateInstance<BrainNodeData>(Data.name);
            EditorUtility.CopySerialized(Data, RuntimeData);
        }

        public override ValidationReport Validate(ValidationReport validationReport)
        {
            // Brain should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                validationReport.Errors.Add("Brain should have both inputs and outputs");
            }

            var inputShapes = GetInputShape();

            // All inputs should be flattened upon reaching brain
            bool correctInputShape = inputShapes.All(s => s.rank == 1);
            if (inputShapes.Count() > 0 && !correctInputShape)
            {
                validationReport.Errors.Add("Inputs to the brain should be flattened");
            }

            return base.Validate(validationReport);
        }
    }
}