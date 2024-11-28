using System;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Models;
using ModularMLAgents.Utilities;

namespace ModularMLAgents.Brain
{
    [System.Serializable]
    public class Brain
    {
        // Brain node is the central part of the agent
        // It processes signals it gets from the sensors and based on that it activates the effectors

        // Here approach is unique as the brain on itself decides what kind of input is important for what effector
        // as he process all the different data it has  

        [HideInInspector] public List<Tensor> Inputs;
        [HideInInspector] public List<Tensor> Outputs;
    }

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

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Brain), canvas);
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
    }
}