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

        this.AddManipulator(AddNodeMenu());
        this.AddManipulator(ContextualGroup());

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    private AgentGraphSearchWindow _searchWindow;

    private IManipulator AddNodeMenu()
    {
        var manipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(
                $"Add node",
                actionEvent => ShowSearchWindow(actionEvent.eventInfo)
            )
        );

        _searchWindow = ScriptableObject.CreateInstance<AgentGraphSearchWindow>();
        _searchWindow.Initialize(this);

        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);

        return manipulator;
    }

    private void ShowSearchWindow(DropdownMenuEventInfo eventInfo)
    {
        var context = new NodeCreationContext
        {
            screenMousePosition = eventInfo.mousePosition,
        };
        nodeCreationRequest?.Invoke(context);
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

    public Node CreateNode(Vector2 position, System.Type nodeType)
    {
        var node = Activator.CreateInstance(nodeType) as AgentGraphNode;
        node.Draw();
        
        node.SetPosition(new Rect(position, Vector2.zero));

        Nodes.Add(node);
        AddElement(node);
        return node;
    }

    private void InitializeDefaultElements()
    {
        var brainNode = CreateNode(Vector2.zero, typeof(BrainNode));
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

                    var connectedEdges = node.Ports.SelectMany(p => p.connections);
                    connectedEdges.ToList().ForEach(e => Edges.Remove(e));
                    elementsToRemove.AddRange(connectedEdges);

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
            Groups.Add(group);

            AddElement(group);
        }

        foreach (AgentGraphNodeData nodeData in data.Nodes)
        {
            var node = nodeData.Load();
            Nodes.Add(node);
            node.Draw();

            AddElement(node);
        }

        foreach (AgentGraphEdgeData edgeData in data.Edges)
        {
            var inputPort = this.GetElementByGuid(edgeData.InputGUID) as Port;
            var outputPort = this.GetElementByGuid(edgeData.OutputGUID) as Port;

            if (inputPort is null || outputPort is null)
            {
                continue;
            }

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

    public AgentGraphData Save(AgentGraphData saveFile)
    {
        Asset = saveFile;

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
