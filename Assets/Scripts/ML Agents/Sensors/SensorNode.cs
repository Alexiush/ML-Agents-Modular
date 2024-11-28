using System;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;

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

        public SensorNode() : base()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Tensor));
            inputPort.name = "Input signal";

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = ScriptableObject.CreateInstance<SensorNodeData>();
            Metadata.Asset = Data;
        }

        public SensorNode(AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as SensorNodeData;
        }

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Sensor), canvas);
        }

        public override IAgentGraphElement Copy()
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = ScriptableObject.CreateInstance<SensorNodeData>();
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new SensorNode(copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw();

            return node;
        }
    }
}
