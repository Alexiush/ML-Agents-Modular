using ModularMLAgents.Models;
using ModularMLAgents.Layers;
using ModularMLAgents.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class Sensor
    {
        // Sensor node passes data to the brain for it to be processed
        // Here sensor is a contract about the shape and type of data that is passed from the source

        [SubclassSelector, SerializeReference]
        public IEncoder Encoder = new Identity();
    }

    [NodePath("Sensor")]
    public class SensorNode : AgentGraphNodeBase<SensorNodeData>
    {
        private Sensor Sensor => (RuntimeData as SensorNodeData).Sensor;

        public SensorNode(AgentGraphContext context, SensorNodeData data) : base(context, data)
        {
            if (data == null)
            {
                Data = context.CreateInstance<SensorNodeData>(GetType().Name);

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

            RuntimeData = context.CreateInstance<SensorNodeData>(Data.name);
            EditorUtility.CopySerialized(Data, RuntimeData);
        }

        public override ValidationReport Validate(ValidationReport validationReport)
        {
            // Sensor should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                validationReport.Errors.Add("Sensor should have both inputs and outputs");
            }

            // Sensor's input should be correct for its encoder
            var inputShape = GetInputShape();
            bool compatibleInputShape = inputShape.Count > 0
                && Sensor.Encoder.Layer.Validate(inputShape, new List<DynamicTensorShape>());
            if (inputShape.Count > 0 && !compatibleInputShape)
            {
                validationReport.Errors.Add("Input does not fit the encoder");
            }

            return base.Validate(validationReport);
        }
    }
}
