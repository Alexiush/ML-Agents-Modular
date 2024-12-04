using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using ModularMLAgents.Utilities;
using Unity.MLAgents.Sensors;
using ModularMLAgents.Models;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace ModularMLAgents.Sensors
{
    [System.Serializable]
    public class Source
    {
        // As the agent works asynchronously it can't use sensor itself to consume input
        // Input source will be an actual input that will eventually be copied onto the sensor

        public Schema OutputShape;
        public ObservationSpec ObservationSpec;
    }

    [NodePath("Source")]
    public class SourceNode : AgentGraphNode
    {
        private Source Source => (Data as SourceNodeData).Source;

        public SourceNode(AgentGraphContext context) : base(context)
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = context.CreateInstance<SourceNodeData>(GetType().Name);
            Metadata.Asset = Data;
        }

        public SourceNode(AgentGraphContext context, AgentGraphElementMetadata metadata) : base(context)
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as SourceNodeData;
        }

        private HashSet<PropertyField> TrackedPropertyFields = new HashSet<PropertyField>();

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Source), canvas);

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
            copyMetadata.Asset = context.CreateInstance<SourceNodeData>(Data.name);
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new SourceNode(context, copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw(context);

            return node;
        }

        public override ValidationReport Validate()
        {
            var errorsList = new List<string>();

            // Source should have output
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                errorsList.Add("Source should have an output");
            }

            var result = new ValidationReport(errorsList);
            ApplyValidationStyle(result);

            return result;
        }

        public override List<TensorShape> GetOutputShapes()
        {
            return new List<TensorShape> { Source.OutputShape.ToShape() };
        }
    }
}