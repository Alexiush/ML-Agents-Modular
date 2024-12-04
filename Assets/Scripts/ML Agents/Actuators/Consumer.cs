using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using Unity.MLAgents.Actuators;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;
using System.Linq;
using Optional;
using System.Collections.Generic;
using ModularMLAgents.Brain;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class Consumer
    {
        // Consumer represents an action buffer where the decoder output will be written

        public ActionSpec ActionSpec;
    }

    [NodePath("Consumer")]
    public class ConsumerNode : AgentGraphNode
    {
        private Consumer Consumer => (Data as ConsumerNodeData).Consumer;

        public ConsumerNode(AgentGraphContext context) : base(context)
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Tensor));
            inputPort.name = "Input signal";

            Data = context.CreateInstance<ConsumerNodeData>(GetType().Name);
            Metadata.Asset = Data;
        }

        public ConsumerNode(AgentGraphContext context, AgentGraphElementMetadata metadata) : base(context)
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as ConsumerNodeData;
        }

        private HashSet<PropertyField> TrackedPropertyFields = new HashSet<PropertyField>();

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Consumer), canvas);

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
            copyMetadata.Asset = context.CreateInstance<ConsumerNodeData>(Data.name);
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new ConsumerNode(context, copyMetadata);
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

            // Consumer should have an input
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                errorsList.Add("Consumer requires an input");
            }

            // Consumer's input should have correct shape for its action model (for now it just should be flat)
            var inputShape = GetInputShape();
            bool correctInputShape = inputShape.HasValue && inputShape.Value.rank == 1;
            if (inputShape.HasValue && !correctInputShape)
            {
                errorsList.Add("Consumer's input should be flattened'");
            }

            var result = new ValidationReport(errorsList);
            ApplyValidationStyle(result);

            return result;
        }

        public override List<TensorShape> GetOutputShapes()
        {
            // Consumer is the end node
            throw new NotImplementedException();
        }
    }
}
