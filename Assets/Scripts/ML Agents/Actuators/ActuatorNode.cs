using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.ComponentModel;


[System.Serializable]
public class Actuator
{
    // Effector node is the one that performs action based on the brain's output
    // Here effector is a contract about the shape and type of data before it goes to consumer - routine that applies the effector

    public Schema InputSchema = new Schema();
    [SubclassSelector, SerializeReference]
    public Decoder Decoder = new IdentityDecoder();
}


[NodePath("Actuator")]
public class ActuatorNode : AgentGraphNode
{
    private ActuatorNodeData Data;
    private Actuator Actuator => Data.Actuator;

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
        SerializedObject serializedObject = new SerializedObject(Metadata.Asset);
        
        SerializedProperty property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do
            {
                var propertyType = typeof(ActuatorNodeData).GetField(property.propertyPath)?.FieldType;
                if (propertyType != typeof(Actuator))
                {
                    continue;
                }

                PropertyField propertyField = new PropertyField(property);
                propertyField.Bind(serializedObject);

                canvas.Add(propertyField);
            }
            while (property.NextVisible(false));
        }
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
        node.Draw();

        return node;
    }
}