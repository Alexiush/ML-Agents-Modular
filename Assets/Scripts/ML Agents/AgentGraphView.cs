using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static TreeEditor.TreeEditorHelper;

public class AgentGraphView : GraphView
{
    private void InitializeBackground()
    {
        var background = new GridBackground();
        background.StretchToParentSize();
        background.name = "AgentGraphBackground";

        Insert(0, background);
    }

    private void InitializeManipulators()
    {
        this.AddManipulator(new ContentZoomer());

        InitializeAddNodeMenus();
        this.AddManipulator(ContextualGroup());

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    private void InitializeAddNodeMenus()
    {
        var nodeTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Contains(typeof(IAgentGraphNode)))
            .Where(t => t.IsDefined(typeof(NodePathAttribute), false))
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (System.Type nodeType in nodeTypes)
        {
            var pathAttribute = Attribute.GetCustomAttribute(nodeType, typeof(NodePathAttribute)) as NodePathAttribute;
            this.AddManipulator(ContextualMenu(pathAttribute.Path, nodeType));
        }
    }

    private IManipulator ContextualGroup()
    {
        var manipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(
                $"Create group",
                actionEvent => AddElement(CreateGroup(actionEvent.eventInfo.localMousePosition, selection.Cast<GraphElement>()))
            )
        );

        return manipulator;
    }

    private Group CreateGroup(Vector2 position, IEnumerable<GraphElement> selection)
    {
        var group = new AgentGraphGroup()
        {
            title = "New group"
        };
        group.SetPosition(new Rect(position, Vector2.zero));
        group.AddElements(selection.Where(e => e is Node or Group));

        return group;
    }

    private IManipulator ContextualMenu(string path, System.Type nodeType)
    {
        var manipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(
                $"Add node/{path}", 
                actionEvent => AddElement(CreateNode(actionEvent.eventInfo.localMousePosition, nodeType))
            )
        );

        return manipulator;
    }

    private Node CreateNode(Vector2 position, System.Type nodeType)
    {
        var node = Activator.CreateInstance(nodeType) as AgentGraphNode;
        node.Draw();
        
        node.SetPosition(new Rect(position, Vector2.zero));

        return node;
    }

    private void InitializeDefaultElements()
    {
        var brainNode = CreateNode(Vector2.zero, typeof(BrainNode));
        AddElement(brainNode);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports
            .Where(p => p.portType == startPort.portType)
            .Where(p => p.node != startPort.node)
            .Where(p => p.capacity != Port.Capacity.Single || !p.connected)
            .ToList();
    }

    public AgentGraphView()
    {
        InitializeManipulators();
        InitializeBackground();
        InitializeDefaultElements();
    }

    private void Load(AgentGraphData data)
    {
        foreach (AgentGraphGroupData groupData in data.Groups)
        {
            // Load group from groupData
            var group = groupData.Load();
            AddElement(group);
        }

        foreach (AgentGraphNodeData nodeData in data.Nodes)
        {
            var node = nodeData.Load();
            node.Draw();

            AddElement(node);
        }
    }

    public AgentGraphView(AgentGraphData data)
    {
        InitializeManipulators();
        InitializeBackground();
        Load(data);
    }

    private void SetTags(GraphElement element, Dictionary<GraphElement, List<object>> tagsDictionary)
    {
        if (element is not Group)
        {
            return;
        }

        var group = element as Group;
        foreach (var groupElement in group.containedElements)
        {
            tagsDictionary.TryAdd(groupElement, new List<object>());
            tagsDictionary[groupElement].Add(group);
        }

        foreach (var groupElement in group.containedElements)
        {
            SetTags(groupElement, tagsDictionary);
        }
    }

    public AgentGraphData Save(AgentGraphData graphData)
    {
        var tagsDictionary = new Dictionary<GraphElement, List<object>>();

        foreach (var element in graphElements)
        {
            tagsDictionary.TryAdd(element, new List<object>());
            tagsDictionary[element].Add(this);
        }

        foreach (var groupElement in graphElements)
        {
            SetTags(groupElement, tagsDictionary);
        }

        var directDescendantTag = new List<object> { this };
        var directDescendants = graphElements.Where(e => tagsDictionary[e].SequenceEqual(directDescendantTag));

        // Save the groups recursively
        graphData.Groups = directDescendants
            .Where(e => e is AgentGraphGroup)
            .Cast<AgentGraphGroup>()
            .Select(g => g.Save(tagsDictionary, graphData))
            .ToList();

        // Filter out nodes that are not grouped and save them
        graphData.Nodes = directDescendants
            .Where(e => e is AgentGraphNode)
            .Cast<AgentGraphNode>()
            .Select(n => n.Save(graphData))
            .ToList();

        return graphData;
    }
}
