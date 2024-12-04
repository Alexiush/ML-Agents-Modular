using System;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;
using System.Linq;
using ModularMLAgents.Actuators;
using System.Collections.Generic;
using Optional;
using UnityEditor.UIElements;
using static Unity.Sentis.Model;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class Sensor
    {
        // Sensor node passes data to the brain for it to be processed
        // Here sensor is a contract about the shape and type of data that is passed from the source

        public Schema InputShape = new Schema();
        [SubclassSelector, SerializeReference]
        public Encoder Encoder = new IdentityEncoder();
    }

    [NodePath("Sensor")]
    public class SensorNode : AgentGraphNode
    {
        private Sensor Sensor => (Data as SensorNodeData).Sensor;

        public SensorNode(AgentGraphContext context) : base(context)
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Tensor));
            inputPort.name = "Input signal";

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = context.CreateInstance<SensorNodeData>(GetType().Name);
            Metadata.Asset = Data;
        }

        public SensorNode(AgentGraphContext context, AgentGraphElementMetadata metadata) : base(context)
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as SensorNodeData;
        }

        private HashSet<PropertyField> TrackedPropertyFields = new HashSet<PropertyField>();

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Sensor), canvas);

            void ValidateOnChange(SerializedPropertyChangeEvent callback)
            {
                Validate();
                Ports.Where(p => p.direction == Direction.Output)
                    .SelectMany(p => p.connections)
                    .Select(e => e.input.node)
                    .ToList()
                    .ForEach(n =>
                    {
                        (n as AgentGraphNode).Validate();
                    });
            }

            var callback = new EventCallback<SerializedPropertyChangeEvent>(ValidateOnChange);

            canvas.RegisterCallback<GeometryChangedEvent>(e =>
            {
                var propertyFields = canvas.Query()
                    .Descendents<PropertyField>()
                    .ToList();

                TrackedPropertyFields.Except(propertyFields)
                    .ToList()
                    .ForEach(p => p.UnregisterCallback<SerializedPropertyChangeEvent>(callback));

                propertyFields.Except(TrackedPropertyFields)
                    .ToList()
                    .ForEach(p =>
                    {
                        TrackedPropertyFields.Add(p);
                        p.RegisterCallback<SerializedPropertyChangeEvent>(callback);
                    });
            });
        }

        public override IAgentGraphElement Copy(AgentGraphContext context)
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = context.CreateInstance<SensorNodeData>(Data.name);
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new SensorNode(context, copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw(context);

            return node;
        }

        private Option<TensorShape> GetInputShape()
        {
            var output = Ports
                .Where(p => p.direction == Direction.Input)
                .SelectMany(p => p.connections)
                .DefaultIfEmpty(null)
                .FirstOrDefault()?
                .output;

            Option<TensorShape> GetNodeOutput(AgentGraphNode node)
            {
                var outputIndex = node.Ports
                    .Where(p => p.direction == Direction.Output)
                    .ToList()
                    .IndexOf(output);
                
                var outputShapes = node.GetOutputShapes();
                if (outputShapes.Count <= outputIndex)
                {
                    return Option.None<TensorShape>();
                }

                var inputShape = outputShapes[outputIndex];
                return Option.Some(inputShape);
            }

            var nodeTyped = output?.node as AgentGraphNode;
            return nodeTyped switch
            {
                null => Option.None<TensorShape>(),
                AgentGraphNode node => GetNodeOutput(node)
            };
        }

        public override ValidationReport Validate()
        {
            var errorsList = new List<string>();

            // Sensor should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                errorsList.Add("Sensor should have both inputs and outputs");
            }    

            // Sensor's input should be correct for its encoder
            var inputShape = GetInputShape();
            bool compatibleInputShape = inputShape.HasValue && Sensor.Encoder.Validate(inputShape.Value);
            if (inputShape.HasValue && !compatibleInputShape)
            {
                errorsList.Add("Input does not fit the encoder");
            }

            var result = new ValidationReport(errorsList);
            ApplyValidationStyle(result);
            
            return result;
        }

        public override List<TensorShape> GetOutputShapes()
        {
            var inputShape = GetInputShape();

            if (!inputShape.HasValue)
            {
                return new List<TensorShape>();
            }

            return new List<TensorShape>
            {
                Sensor.Encoder.GetShape(inputShape.Value)
            };
        }
    }
}
