using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Brain
{
    // Brain node is the central part of the agent
    // It processes signals it gets from the sensors and based on that it activates the effectors

    // Here approach is unique as the brain on itself decides what kind of input is important for what effector
    // as he process all the different data it has  

    public List<Schema> Inputs;
    public List<Schema> Outputs;
}

public class BrainNode : AgentGraphNode
{
    private Brain _brain = new Brain();

    public BrainNode() : base() { }

    public BrainNode(AgentGraphElementMetadata metadata) : base() 
    { 
        Metadata = metadata;
    }

    public override void Draw()
    {
        titleContainer.Q<Label>("title-label").text = "Brain";

        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Schema));
        inputPort.name = "Input signals";
        inputContainer.Add(inputPort);

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Schema));
        outputPort.name = "Output signals";
        outputContainer.Add(outputPort);
    }

    public override AgentGraphNodeData Save(UnityEngine.Object parent)
    {
        var data = Metadata.Asset as BrainNodeData;
        if (data is null)
        {
            data = ScriptableObject.CreateInstance<BrainNodeData>();
            AssetDatabase.AddObjectToAsset(data, parent);
            Metadata.Asset = data;
        }

        data.Metadata = Metadata;
        data.Brain = _brain;

        return data;
    }
}