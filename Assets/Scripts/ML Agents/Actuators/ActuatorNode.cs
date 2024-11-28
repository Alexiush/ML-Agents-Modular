using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;

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
        private Actuator Actuator => (Data as ActuatorNodeData).Actuator;

        public ActuatorNode() : base()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Tensor));
            inputPort.name = "Input signal";

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
            outputPort.name = "Output signal";

            Data = ScriptableObject.CreateInstance<ActuatorNodeData>();
            Metadata.Asset = Data;
        }

        public ActuatorNode(AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = Metadata.Asset as ActuatorNodeData;
        }

        public override void DrawParameters(VisualElement canvas)
        {
            InspectorUtilities.DrawFilteredProperties(Metadata.Asset, field => field?.FieldType == typeof(Actuator), canvas);
        }

        public override void Draw()
        {
            titleContainer.Q<Label>("title-label").text = "Actuator";

            foreach (var port in Ports.Where(p => p.direction == Direction.Input))
            {
                inputContainer.Add(port);
            }

            foreach (var port in Ports.Where(p => p.direction == Direction.Output))
            {
                outputContainer.Add(port);
            }

            VisualElement container = new VisualElement();
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;

            DrawParameters(container);

            this.extensionContainer.Add(container);
            RefreshExpandedState();
        }

        public override AgentGraphNodeData Save(UnityEngine.Object parent)
        {
            if (!AssetDatabase.Contains(Data))
            {
                AssetDatabase.AddObjectToAsset(Data, parent);
            }

            Data.Metadata = Metadata;
            Data.Ports = Ports.Select(p => new AgentGraphPortData(p)).ToList();

            return Data;
        }

        public override IAgentGraphElement Copy()
        {
            var copyMetadata = Metadata;
            copyMetadata.Asset = ScriptableObject.CreateInstance<ActuatorNodeData>();
            copyMetadata.GUID = Guid.NewGuid().ToString();

            var node = new ActuatorNode(copyMetadata);
            Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw();

            return node;
        }
    }
}