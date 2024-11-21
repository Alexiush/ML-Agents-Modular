using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Sensor
{
    // Sensor node passes data to the brain for it to be processed
    // Here sensor is a contract about the shape and type of data that is passed from the source

    public Schema InputSchema = new Schema();
    [SubclassSelector, SerializeReference]
    public Encoder Encoder = new IdentityEncoder();
}

[NodePath("Sensor")]
public class SensorNode : AgentGraphNode
{
    private SensorNodeData Data;
    private Sensor Sensor => Data.Sensor;

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
        SerializedObject serializedObject = new SerializedObject(Metadata.Asset);

        SerializedProperty property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do
            {
                var propertyType = typeof(SensorNodeData).GetField(property.propertyPath)?.FieldType;
                if (propertyType != typeof(Sensor))
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
        titleContainer.Q<Label>("title-label").text = "Sensor";

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
        copyMetadata.Asset = ScriptableObject.CreateInstance<SensorNodeData>();
        copyMetadata.GUID = Guid.NewGuid().ToString();

        var node = new SensorNode(copyMetadata);
        Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
        node.Draw();

        return node;
    }
}
