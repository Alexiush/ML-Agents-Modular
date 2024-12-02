using System;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Models;
using ModularMLAgents.Utilities;
using System.Linq;
using ModularMLAgents.Actuators;
using UnityEditor.UIElements;

namespace ModularMLAgents.Brain
{
    public class BrainNode : AgentGraphNode
    {
        private Brain Brain => (Data as BrainNodeData).Brain;

        public BrainNode() : base()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Tensor));
            inputPort.name = "Input signal";

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = ScriptableObject.CreateInstance<BrainNodeData>();
            Metadata.Asset = Data;
        }

        public BrainNode(AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as BrainNodeData;
        }

        private HashSet<PropertyField> TrackedPropertyFields = new HashSet<PropertyField>();

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Brain), canvas);

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

        public override IAgentGraphElement Copy()
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = ScriptableObject.CreateInstance<BrainNodeData>();
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new BrainNode(copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw();

            return node;
        }

        public override ValidationReport Validate()
        {
            var errorsList = new List<string>();

            // Brain should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                errorsList.Add("Brain should have both inputs and outputs");
            }

            var inputShapes = Ports
                .Where(p => p.direction == Direction.Input)
                .First()
                .connections
                .Select(e =>
                {
                    var nodeTyped = e.output?.node as AgentGraphNode;
                    return nodeTyped?.GetOutputShapes();
                });

            // All inputs should be flattened upon reaching brain
            bool correctInputShape = inputShapes.All(s => s is not null && s.Count() == 1 && s[0].rank == 1);
            if (inputShapes.Count() > 0 && !correctInputShape)
            {
                errorsList.Add("Inputs to the brain should be flattened");
            }

            var result = new ValidationReport(errorsList);
            ApplyValidationStyle(result);

            return result;
        }

        public override List<TensorShape> GetOutputShapes()
        {
            return Ports
                .Where(p => p.direction == Direction.Output)
                .First()
                .connections
                .Select(e =>
                {
                    var nodeTyped = e.input?.node as ActuatorNode;
                    return nodeTyped.Actuator.InputShape.ToShape();
                })
                .ToList();
        }
    }
}