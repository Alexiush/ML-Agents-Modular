using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public interface IAgentGraphNode
{
    public void Draw();
}

public interface IAgentGraphElement
{
    public abstract AgentGraphElementMetadata GetMetadata();
}

public abstract class AgentGraphNode : Node, IAgentGraphNode, IAgentGraphElement
{
    public abstract void Draw();

    protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
    public virtual AgentGraphElementMetadata GetMetadata() => Metadata;
    
    public override void SetPosition(Rect newPos)
    {
        Metadata.Position = newPos;
        base.SetPosition(newPos);
    }

    public abstract AgentGraphNodeData Save(UnityEngine.Object parent);
}

public class AgentGraphGroup : Group, IAgentGraphElement
{
    protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
    public virtual AgentGraphElementMetadata GetMetadata() => Metadata;

    public AgentGraphGroup() : base() { }

    public AgentGraphGroup(AgentGraphElementMetadata metadata) : base()
    {
        Metadata = metadata;
    }

    public override void SetPosition(Rect newPos)
    {
        Metadata.Position = newPos;
        base.SetPosition(newPos);
    }

    public AgentGraphGroupData Save(Dictionary<GraphElement, List<object>> tagsDictionary, UnityEngine.Object rootAsset)
    {
        var groupData = Metadata.Asset as AgentGraphGroupData;
        if (groupData is null)
        {
            groupData = ScriptableObject.CreateInstance<AgentGraphGroupData>();
            AssetDatabase.AddObjectToAsset(groupData, rootAsset);
            Metadata.Asset = groupData;
        }

        var directDescendantTag = tagsDictionary[this].Concat(new List<object> { this });
        var directDescendants = this.containedElements.Where(e => tagsDictionary[e].SequenceEqual(directDescendantTag));

        // Save the groups recursively
        groupData.Groups = directDescendants
            .Where(e => e is AgentGraphGroup)
            .Cast<AgentGraphGroup>()
            .Select(g => g.Save(tagsDictionary, groupData))
            .ToList();

        // Filter out nodes that are not grouped and save them
        groupData.Nodes = directDescendants
            .Where(e => e is AgentGraphNode)
            .Cast<AgentGraphNode>()
            .Select(n => n.Save(groupData))
            .ToList();

        groupData.Metadata = Metadata;

        return groupData;
    }
}
