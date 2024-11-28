using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using Unity.MLAgents.Actuators;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;

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

        public ConsumerNode() : base()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Tensor));
            inputPort.name = "Input signal";

            Data = ScriptableObject.CreateInstance<ConsumerNodeData>();
            Metadata.Asset = Data;
        }

        public ConsumerNode(AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as ConsumerNodeData;
        }

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Consumer), canvas);
        }

        public override IAgentGraphElement Copy()
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = ScriptableObject.CreateInstance<ConsumerNodeData>();
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new ConsumerNode(copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw();

            return node;
        }
    }
}
