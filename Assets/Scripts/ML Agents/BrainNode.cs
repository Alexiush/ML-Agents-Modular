using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
    private BrainNodeData Data;
    private Brain Brain => Data.Brain;

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

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Brain";

        foreach (var port in Ports.Where(p => p.direction == Direction.Input))
        {
            inputContainer.Add(port);
        }

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
                var propertyType = typeof(BrainNodeData).GetField(property.propertyPath)?.FieldType;                
                if (propertyType != typeof(Brain))
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
        copyMetadata.Asset = ScriptableObject.CreateInstance<BrainNodeData>();
        copyMetadata.GUID = Guid.NewGuid().ToString();

        var node = new BrainNode(copyMetadata);
        node.Draw();

        return node;
    }
}