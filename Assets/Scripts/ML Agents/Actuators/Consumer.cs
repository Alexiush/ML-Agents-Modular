using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

[System.Serializable]
public class Consumer
{
    // Consumer represents an action buffer where the decoder output will be written

    public Schema Schema;
    public ActionSpec ActionSpec;
}

[NodePath("Consumer")]
public class ConsumerNode : AgentGraphNode
{
    private ConsumerNodeData Data;
    private Consumer Consumer => Data.Consumer;

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
        SerializedObject serializedObject = new SerializedObject(Metadata.Asset);

        SerializedProperty property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do
            {
                var propertyType = typeof(ConsumerNodeData).GetField(property.propertyPath)?.FieldType;
                if (propertyType != typeof(Consumer))
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
        titleContainer.Q<Label>("title-label").text = "Consumer";

        foreach (var port in Ports.Where(p => p.direction == Direction.Input))
        {
            inputContainer.Add(port);
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
        copyMetadata.Asset = ScriptableObject.CreateInstance<ConsumerNodeData>();
        copyMetadata.GUID = Guid.NewGuid().ToString();

        var node = new ConsumerNode(copyMetadata);
        Ports.ForEach(p => node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
        node.Draw();

        return node;
    }
}
