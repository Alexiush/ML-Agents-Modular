using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;

[System.Serializable]
public class InputSource
{
    // As the agent works asynchronously it can't use sensor itself to consume input
    // Input source will be an actual input that will eventually be copied onto the sensor

    public Schema Schema;
}

[NodePath("Source")]
public class SourceNode : AgentGraphNode
{
    private SourceNodeData Data;
    private InputSource Source => Data.Source;

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

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Source";

        foreach (var port in Ports.Where(p => p.direction == Direction.Output))
        {
            outputContainer.Add(port);
        }

        SerializedObject serializedObject = new SerializedObject(Metadata.Asset);

        VisualElement container = new VisualElement();
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;

        SerializedProperty property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do
            {
                var propertyType = typeof(SourceNodeData).GetField(property.propertyPath)?.FieldType;
                if (propertyType != typeof(InputSource))
                {
                    continue;
                }

                PropertyField propertyField = new PropertyField(property);
                propertyField.Bind(serializedObject);

                container.Add(propertyField);
            }
            while (property.NextVisible(false));
        }

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
        copyMetadata.Asset = ScriptableObject.CreateInstance<SourceNodeData>();
        copyMetadata.GUID = Guid.NewGuid().ToString();

        var node = new SourceNode(copyMetadata);
        node.Draw();

        return node;
    }
}
