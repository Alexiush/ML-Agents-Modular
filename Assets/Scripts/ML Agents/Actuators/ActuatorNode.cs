using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;
using System.Collections.Generic;
using Optional;
using ModularMLAgents.Brain;

namespace ModularMLAgents.Actuators
{
    [System.Serializable]
    public class Actuator
    {
        // Effector node is the one that performs action based on the brain's output
        // Here effector is a contract about the shape and type of data before it goes to consumer - routine that applies the effector

        public Schema InputShape = new Schema();
        [SubclassSelector, SerializeReference]
        public Decoder Decoder = new IdentityDecoder();
    }


    [NodePath("Actuator")]
    public class ActuatorNode : AgentGraphNode
    {
        public Actuator Actuator => (Data as ActuatorNodeData).Actuator;

        public ActuatorNode(AgentGraphContext context) : base(context)
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Tensor));
            inputPort.name = "Input signal";

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = context.CreateInstance<ActuatorNodeData>(GetType().Name);
            Metadata.Asset = Data;
        }

        public ActuatorNode(AgentGraphContext context, AgentGraphElementMetadata metadata) : base(context)
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as ActuatorNodeData;
        }

        private HashSet<PropertyField> TrackedPropertyFields = new HashSet<PropertyField>();

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Actuator), canvas);

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
            copyMetadata.Asset = context.CreateInstance<ActuatorNodeData>(Data.name);
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new ActuatorNode(context, copyMetadata);
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
                BrainNode brain => Option.Some(Actuator.InputShape.ToShape()),
                AgentGraphNode node => GetNodeOutput(node)
            };
        }

        public override ValidationReport Validate()
        {
            var errorsList = new List<string>();

            // Actuator should have both connections
            bool allPortsConnected = Ports.All(p => p.connected);
            if (!allPortsConnected)
            {
                errorsList.Add("Actuator should have both input and output");
            }

            // Actuator's input should be either from the brain or have corresponding shape
            var inputShape = GetInputShape();
            bool correctInputShape = inputShape.HasValue && inputShape.Value == Actuator.InputShape.ToShape();
            if (inputShape.HasValue && !correctInputShape)
            {
                errorsList.Add("Input has wrong shape");
            }

            // Input should also be correct for its Decoder
            bool compatibleInputShape = inputShape.HasValue && Actuator.Decoder.Validate(inputShape.Value);
            if (inputShape.HasValue && !compatibleInputShape)
            {
                errorsList.Add("Input does not fit the decoder");
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
                Actuator.Decoder.GetShape(inputShape.Value)
            };
        }
    }
}