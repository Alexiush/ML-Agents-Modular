using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public interface IAgentGraphNode
{
    public void Draw();
}

public interface IAgentGraphElement
{
    public abstract AgentGraphElementMetadata GetMetadata();
    public abstract GraphElement GetParentComposite();
    public abstract void SetParentComposite(GraphElement parentComposite);
    public abstract IAgentGraphElement Copy();
}

public abstract class AgentGraphNode : Node, IAgentGraphNode, IAgentGraphElement
{
    public List<Port> Ports = new List<Port>();

    protected override void OnPortRemoved(Port port)
    {
        Ports.Remove(port);
        base.OnPortRemoved(port);
    }

    protected GraphElement ParentComposite;
    public GraphElement GetParentComposite() => ParentComposite;
    public void SetParentComposite(GraphElement parentComposite)
    {
        ParentComposite = parentComposite;
    }

    public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, System.Type type)
    {
        var port = base.InstantiatePort(orientation, direction, capacity, type);

        Ports.Add(port);
        return port;
    }

    public abstract void DrawParameters(VisualElement canvas);
    public abstract void Draw();

    protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
    public virtual AgentGraphElementMetadata GetMetadata() => Metadata;
    
    public override void SetPosition(Rect newPos)
    {
        Metadata.Position = newPos;
        base.SetPosition(newPos);
    }

    public abstract AgentGraphNodeData Save(UnityEngine.Object parent);

    public abstract IAgentGraphElement Copy();
}

public class AgentGraphGroup : Group, IAgentGraphElement
{
    protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
    public virtual AgentGraphElementMetadata GetMetadata() => Metadata;

    public AgentGraphGroup() : base() { }

    public AgentGraphGroup(AgentGraphElementMetadata metadata) : base()
    {
        viewDataKey = metadata.GUID;
        Metadata = metadata;
    }

    protected GraphElement ParentComposite;
    public GraphElement GetParentComposite() => ParentComposite;
    public void SetParentComposite(GraphElement parentComposite)
    {
        ParentComposite = parentComposite;
    }

    public override void SetPosition(Rect newPos)
    {
        Metadata.Position = newPos;
        base.SetPosition(newPos);
    }

    public List<AgentGraphNode> Nodes = new List<AgentGraphNode>();
    public List<AgentGraphGroup> Groups = new List<AgentGraphGroup>();

    public AgentGraphGroupData Save(UnityEngine.Object rootAsset)
    {
        var groupData = Metadata.Asset as AgentGraphGroupData;
        if (groupData is null)
        {
            groupData = ScriptableObject.CreateInstance<AgentGraphGroupData>();
            AssetDatabase.AddObjectToAsset(groupData, rootAsset);
            Metadata.Asset = groupData;
        }

        groupData.Groups = Groups
            .Select(g => g.Save(groupData))
            .ToList();

        groupData.Nodes = Nodes
            .Select(n => n.Save(groupData))
            .ToList();

        groupData.Metadata = Metadata;

        return groupData;
    }

    public IAgentGraphElement Copy()
    {
        var copyMetadata = Metadata;
        copyMetadata.Asset = ScriptableObject.CreateInstance<AgentGraphGroupData>();
        copyMetadata.GUID = Guid.NewGuid().ToString();

        return new AgentGraphGroup(copyMetadata);
    }
}
