using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using ModularMLAgents.Utilities;
using Unity.MLAgents.Sensors;
using ModularMLAgents.Models;

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

        public SourceNode() : base()
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = ScriptableObject.CreateInstance<SourceNodeData>();
            Metadata.Asset = Data;
        }

        public SourceNode(AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as SourceNodeData;
        }

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Source), canvas);
        }

        public override IAgentGraphElement Copy()
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = ScriptableObject.CreateInstance<SourceNodeData>();
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new SourceNode(copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw();

            return node;
        }
    }
}