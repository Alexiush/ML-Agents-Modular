using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

[System.Serializable]
public class Consumer
{
    // Consumer represents an action buffer where the decoder output will be written

    public Schema Schema;
}

[NodePath("Consumer")]
public class ConsumerNode : AgentGraphNode
{
    private ConsumerNodeData Data;
    private Consumer Consumer => Data.Consumer;

    public ConsumerNode() : base()
    {
        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Tensor));
        outputPort.name = "Output signal";

        Data = ScriptableObject.CreateInstance<ConsumerNodeData>();
        Metadata.Asset = Data;
    }

    public ConsumerNode(AgentGraphElementMetadata metadata) : base()
    {
        viewDataKey = metadata.GUID;
        Metadata = metadata;

        Data = Metadata.Asset as ConsumerNodeData;
    }

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Consumer";

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
                var propertyType = typeof(ConsumerNodeData).GetField(property.propertyPath)?.FieldType;
                if (propertyType != typeof(Consumer))
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
}
