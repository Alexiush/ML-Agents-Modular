using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
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

        Groups.Add(group);
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

        Nodes.Add(node);
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

    private HashSet<AgentGraphNode> Nodes = new HashSet<AgentGraphNode>();
    private HashSet<AgentGraphGroup> Groups = new HashSet<AgentGraphGroup>();
    private HashSet<Edge> Edges = new HashSet<Edge>();

    private void OnElementDeleted(string operation, AskUser askUser)
    {
        // ask user and operations are omitted for now
        List<GraphElement> elementsToRemove = new List<GraphElement>();

        foreach (var element in selection)
        {
            switch (element)
            {
                case AgentGraphNode node:
                    switch (node.GetParentComposite())
                    {
                        case AgentGraphGroup parent:
                            parent.Nodes.Remove(node);
                            break;
                        case null:
                            Nodes.Remove(node);

                            var asset = node.GetMetadata().Asset;
                            Debug.Log(asset);

                            if (asset != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(asset);
                                EditorUtility.SetDirty(Asset);
                            }
                            break;
                        default:
                            // pass
                            break;
                    }

                    elementsToRemove.Add(node);
                    break;

                case AgentGraphGroup group:
                    switch (group.GetParentComposite())
                    {
                        case AgentGraphGroup parent:
                            parent.Groups.Remove(group);
                            break;
                        case null:
                            Groups.Remove(group);

                            var asset = group.GetMetadata().Asset;
                            if (asset != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(asset);
                                EditorUtility.SetDirty(Asset);
                            }
                            break;
                        default:
                            // pass
                            break;
                    }

                    elementsToRemove.Add(group);                    
                    break;

                case Edge edge:
                    Edges.Remove(edge);
                    elementsToRemove.Add(edge);
                    
                    break;
                default:
                    // Pass
                    break;
            }
        }

        elementsToRemove.ForEach(e => RemoveElement(e));
    }

    private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
    {
        var groupTyped = group as AgentGraphGroup;

        if (groupTyped is null)
        {
            return;
        }

        foreach (var element in elements)
        {
            switch (element)
            {
                case AgentGraphNode node:
                    if (node.GetParentComposite() is not null)
                    {
                        break;
                    }

                    Nodes.Remove(node);
                    node.SetParentComposite(groupTyped);

                    groupTyped.Nodes.Add(node);

                    break;

                case AgentGraphGroup nestedGroup:
                    if (nestedGroup.GetParentComposite() is not null)
                    {
                        break;
                    }

                    Groups.Remove(nestedGroup);
                    nestedGroup.SetParentComposite(groupTyped);

                    groupTyped.Groups.Add(nestedGroup);

                    break;

                default:
                    // Pass
                    break;
            }
        }
    }

    private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
    {
        var groupTyped = group as AgentGraphGroup;

        if (groupTyped is null)
        {
            return;
        }

        foreach (var element in elements)
        {
            switch (element)
            {
                case AgentGraphNode node:
                    if (node.GetParentComposite() != group)
                    {
                        break;
                    }

                    Nodes.Add(node);
                    node.SetParentComposite(null);

                    groupTyped.Nodes.Remove(node);

                    break;

                case AgentGraphGroup nestedGroup:
                    if (nestedGroup.GetParentComposite() != group)
                    {
                        break;
                    }

                    Groups.Add(nestedGroup);
                    nestedGroup.SetParentComposite(null);

                    groupTyped.Groups.Remove(nestedGroup);

                    break;

                default:
                    // Pass
                    break;
            }
        }
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
    {
        // No preprocess steps yet

        return changes;
    }

    private void InitializeCallbacks()
    {
        deleteSelection = (op, askUser) => OnElementDeleted(op, askUser);
        // elementResized
        elementsAddedToGroup = (group, elements) => OnElementsAddedToGroup(group, elements);
        // elementsInsertedToStackNode
        elementsRemovedFromGroup = (group, elements) => OnElementsRemovedFromGroup(group, elements);
        // elementsRemovedFromStackNode
        // groupTitleChanged
        graphViewChanged = (changes) => OnGraphViewChanged(changes);
        // nodeCreationRequest
        // serializeGraphElements
        // unserializeAndPaste
        // viewTransformChanged
    }

    public AgentGraphView()
    {
        InitializeManipulators();
        InitializeBackground();
        InitializeCallbacks();
        InitializeDefaultElements();
    }

    private void Load(AgentGraphData data)
    {
        foreach (AgentGraphGroupData groupData in data.Groups)
        {
            var group = groupData.Load();
            AddElement(group);
        }

        foreach (AgentGraphNodeData nodeData in data.Nodes)
        {
            var node = nodeData.Load();
            node.Draw();

            AddElement(node);
        }

        foreach (AgentGraphEdgeData edgeData in data.Edges)
        {
            var inputPort = this.GetElementByGuid(edgeData.InputGUID) as Port;
            var outputPort = this.GetElementByGuid(edgeData.OutputGUID) as Port;

            var edge = inputPort.ConnectTo(outputPort);
            AddElement(edge);
        }
    }

    protected AgentGraphData Asset;

    public AgentGraphView(AgentGraphData data)
    {
        InitializeManipulators();
        InitializeBackground();
        InitializeCallbacks();

        Asset = data;
        Load(Asset);
    }

    public AgentGraphData Save()
    {
        Asset.Groups = Groups
            .Select(g => g.Save(Asset))
            .ToList();

        Asset.Nodes = Nodes
            .Select(n => n.Save(Asset))
            .ToList();

        Asset.Edges = edges
            .Where(e => e.input.node as AgentGraphNode is not null)
            .Where(e => e.output.node as AgentGraphNode is not null)
            .Select(e => new AgentGraphEdgeData(e))
            .ToList();

        return Asset;
    }
}
