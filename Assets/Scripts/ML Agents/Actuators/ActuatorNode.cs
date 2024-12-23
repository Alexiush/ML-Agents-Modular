using ModularMLAgents.Layers;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class Actuator
    {
        // Effector node is the one that performs action based on the brain's output
        // Here effector is a contract about the shape and type of data before it goes to consumer - routine that applies the effector

        public Schema InputShape = new Schema();
        [SubclassSelector, SerializeReference]
        public IDecoder Decoder = new Identity();
    }


    [NodePath("Actuator")]
    public class ActuatorNode : AgentGraphNodeBase<ActuatorNodeData>
    {
        public Actuator Actuator => (RuntimeData as ActuatorNodeData).Actuator;

        public ActuatorNode(AgentGraphContext context, ActuatorNodeData data = null) : base(context, data)
        {
            if (data is null)
            {
                Data = context.CreateInstance<ActuatorNodeData>(GetType().Name);

                Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Tensor));
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

            RuntimeData = context.CreateInstance<ActuatorNodeData>(Data.name);
            EditorUtility.CopySerialized(Data, RuntimeData);
        }

        public override ValidationReport Validate(ValidationReport validationReport)
        {
            // Actuator should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                validationReport.Errors.Add("Actuator should have both input and output");
            }

            // Actuator's input should be either from the brain or have corresponding shape
            var inputShape = GetInputShape();
            bool correctInputShape = inputShape.Count == 1 && inputShape[0] == Actuator.InputShape.AsTensorShape();
            if (inputShape.Count > 0 && !correctInputShape)
            {
                validationReport.Errors.Add("Input has wrong shape");
            }

            // Input should also be correct for its Decoder
            bool compatibleInputShape = inputShape.Count > 0
                && Actuator.Decoder.Layer.Validate(inputShape, new List<TensorShape>());
            if (inputShape.Count > 0 && !compatibleInputShape)
            {
                validationReport.Errors.Add("Input does not fit the decoder");
            }

            return base.Validate(validationReport);
        }
    }
}