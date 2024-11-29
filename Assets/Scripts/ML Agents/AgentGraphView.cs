using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Editor;
using ModularMLAgents.Models;
using ModularMLAgents.Brain;
using UnityEditor.PackageManager.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace ModularMLAgents
{
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
            this.AddManipulator(RemoveFromGroup());

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
                    actionEvent => ConfigureAndAddElement(CreateGroup(actionEvent.eventInfo.localMousePosition, selection.Cast<GraphElement>()))
                )
            );

            return manipulator;
        }

        private IManipulator RemoveFromGroup()
        {
            var manipulator = new ContextualMenuManipulator(
                menuEvent =>
                {
                    var ancestors = selection
                        .Where(e => e is IAgentGraphElement)
                        .Cast<IAgentGraphElement>()
                        .Select(e => e.GetParentComposite());

                    if (ancestors.Count() == 0)
                    {
                        return;
                    }

                    GraphElement commonAncestor = ancestors.First();
                    if (commonAncestor is null || ancestors.Any(e => e != commonAncestor))
                    {
                        return;
                    }

                    menuEvent.menu.AppendAction(
                        $"Remove from group",
                        actionEvent => (commonAncestor as Group).RemoveElements(selection.Cast<GraphElement>())
                    );
                }
            );

            return manipulator;
        }

        private void ConfigureCapabilities(GraphElement element)
        {
            switch (element)
            {
                case BrainNode brainNode:
                    brainNode.capabilities =
                        Capabilities.Selectable
                        | Capabilities.Collapsible
                        | Capabilities.Renamable
                        | Capabilities.Ascendable
                        | Capabilities.Movable
                        | Capabilities.Snappable;
                    break;

                default:
                    break;
            }
        }

        private void AddElementAndRegister(GraphElement element)
        {
            switch (element)
            {
                case AgentGraphNode agentGraphNode:
                    Nodes.Add(agentGraphNode);
                    break;
                case AgentGraphGroup agentGraphGroup:
                    Groups.Add(agentGraphGroup);
                    break;
                default:
                    break;
            }

            AddElement(element);
        }

        private void ConfigureAndAddElement(GraphElement element, bool asCommand = true)
        {
            if (asCommand)
            {
                RegisterUndoRedoEvent($"Add {element.GetType()}",
                    onUndo: () => OnElementsDeleted("AddElement", AskUser.DontAskUser, new List<ISelectable> { element }),
                    onRedo: () => ConfigureAndAddElement(element)
                );
            }

            ConfigureCapabilities(element);
            AddElementAndRegister(element);
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

        public Node CreateNode(Vector2 position, System.Type nodeType)
        {
            var node = Activator.CreateInstance(nodeType) as AgentGraphNode;
            node.Draw();

            node.SetPosition(new Rect(position, Vector2.zero));

            ConfigureAndAddElement(node);
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

        private HashSet<IAgentGraphElement> ElementsToRemove = new HashSet<IAgentGraphElement>();

        private void ApplyRemove()
        {
            foreach (var e in ElementsToRemove)
            {
                var asset = e.GetMetadata().Asset;
                if (asset != null)
                {
                    AssetDatabase.RemoveObjectFromAsset(asset);
                    EditorUtility.SetDirty(Asset);
                }
            }

            ElementsToRemove.Clear();
        }

        private Dictionary<string, Action> _undoActions = new Dictionary<string, Action>();
        private Dictionary<string, Action> _redoActions = new Dictionary<string, Action>();

        private void OnUndoRedoEvent(in UndoRedoInfo info)
        {
            var id = $"{info.undoGroup}/{info.undoName}";
            Action action = null;

            var commandSet = info.isRedo ? _redoActions : _undoActions;
            commandSet.TryGetValue(id, out action);

            action?.Invoke();
        }

        private void RegisterUndoRedoEvent(string commandName, Action onUndo, Action onRedo)
        {
            var id = $"{Undo.GetCurrentGroup()}/{commandName}";
            Undo.RecordObject(_versionTracker, commandName);
            _versionTracker.Version++;

            Action undoAndClear = () =>
            {
                onUndo?.Invoke();
            };
            _undoActions.Add(id, undoAndClear);

            Action redoAndClear = () =>
            {
                onRedo?.Invoke();
            };
            _redoActions.Add(id, redoAndClear);
        }

        private void RestoreElement(GraphElement element)
        {
            // Same as configure + add, but with remove set check

            if (element is IAgentGraphElement agentGraphElement)
            {
                ElementsToRemove.Remove(agentGraphElement);
            }
            ConfigureAndAddElement(element, false);
        }

        private void OnElementsDeleted(string operation, AskUser askUser, List<ISelectable> elementsToDelete, bool asCommand = true)
        {
            // ask user and operations are omitted for now
            List<GraphElement> elementsToRemove = new List<GraphElement>();
            var selectionCopy = new List<ISelectable>(elementsToDelete);
            Stack<Action> _undoStack = new Stack<Action>();

            foreach (var element in selectionCopy)
            {
                switch (element)
                {
                    case AgentGraphNode node:
                        if ((node.capabilities & Capabilities.Deletable) == 0)
                        {
                            continue;
                        }

                        switch (node.GetParentComposite())
                        {
                            case AgentGraphGroup parent:
                                _undoStack.Push(() => AddToComposite(parent, new List<GraphElement> { node }));
                                parent.Nodes.Remove(node);
                                break;

                            case null:
                                Nodes.Remove(node);
                                ElementsToRemove.Add(node);
                                break;

                            default:
                                // pass
                                break;
                        }

                        _undoStack.Push(() => RestoreElement(node));
                        elementsToRemove.Add(node);

                        // No need to remove the edges, only disconnect them
                        node.Ports
                            .SelectMany(p => p.connections)
                            .ToList()
                            .ForEach(edge =>
                            {
                                _undoStack.Push(() =>
                                {
                                    edge.input.Connect(edge);
                                    edge.output.Connect(edge);
                                    RestoreElement(edge);
                                });

                                edge.input.Disconnect(edge);
                                edge.output.Disconnect(edge);

                                elementsToRemove.Add(edge);
                            });

                        break;

                    case AgentGraphGroup group:
                        if ((group.capabilities & Capabilities.Deletable) == 0)
                        {
                            continue;
                        }

                        switch (group.GetParentComposite())
                        {
                            case AgentGraphGroup parent:
                                _undoStack.Push(() => AddToComposite(parent, new List<GraphElement> { group }));
                                parent.Groups.Remove(group);
                                break;

                            case null:
                                Groups.Remove(group);
                                ElementsToRemove.Add(group);
                                break;

                            default:
                                // pass
                                break;
                        }

                        _undoStack.Push(() => RestoreElement(group));
                        elementsToRemove.Add(group);
                        break;

                    case Edge edge:
                        _undoStack.Push(() =>
                        {
                            edge.input.Connect(edge);
                            edge.output.Connect(edge);
                            RestoreElement(edge);
                        });

                        edge.input.Disconnect(edge);
                        edge.output.Disconnect(edge);

                        elementsToRemove.Add(edge);

                        break;

                    default:
                        // Pass
                        break;
                }
            }

            if (asCommand)
            {
                RegisterUndoRedoEvent("Delete selected",
                    onUndo: () => _undoStack.ToList().ForEach(a => a?.Invoke()),
                    onRedo: () => OnElementsDeleted(operation, askUser, selectionCopy)
                );
            }

            elementsToRemove.ForEach(e => RemoveElement(e));
        }

        private void AddToComposite(Group group, IEnumerable<GraphElement> elements)
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
                        if ((node.capabilities & Capabilities.Groupable) == 0)
                        {
                            continue;
                        }

                        if (node.GetParentComposite() is not null)
                        {
                            break;
                        }

                        Nodes.Remove(node);
                        node.SetParentComposite(groupTyped);

                        groupTyped.Nodes.Add(node);

                        break;

                    case AgentGraphGroup nestedGroup:
                        if ((nestedGroup.capabilities & Capabilities.Deletable) == 0)
                        {
                            continue;
                        }

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

                if (!groupTyped.ContainsElement(element))
                {
                    groupTyped.AddElement(element);
                }
            }
        }

        private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            RegisterUndoRedoEvent("Add to group",
                onUndo: () => RemoveFromComposite(group, elements),
                onRedo: () => AddToComposite(group, elements)
            );

            AddToComposite(group, elements);
        }

        private void RemoveFromComposite(Group group, IEnumerable<GraphElement> elements)
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

                if (groupTyped.ContainsElement(element))
                {
                    groupTyped.RemoveElement(element);
                }
            }
        }

        private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
        {
            RegisterUndoRedoEvent("Remove from group",
                onUndo: () => AddToComposite(group, elements),
                onRedo: () => RemoveFromComposite(group, elements)
            );

            RemoveFromComposite(group, elements);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            // No preprocess steps yet
            return changes;
        }

        private Dictionary<string, IEnumerable<GraphElement>> _copyPasteBuffer = new Dictionary<string, IEnumerable<GraphElement>>();

        private string CopyElements(IEnumerable<GraphElement> elements)
        {
            string id = Guid.NewGuid().ToString();
            var copies = elements
                .Where(e => e is IAgentGraphElement)
                .Cast<IAgentGraphElement>()
                .Select(e => e.Copy())
                .Cast<GraphElement>();

            _copyPasteBuffer.Add(id, copies);

            return id;
        }

        private bool ValidatePaste(string serializedData)
        {
            // No checks for now

            return _copyPasteBuffer.ContainsKey(serializedData);
        }

        private void Paste(string operation, string serializedData, bool asCommand = true)
        {
            List<GraphElement> copies = _copyPasteBuffer[serializedData]
                .Cast<IAgentGraphElement>()
                .Select(e => e.Copy())
                .Cast<GraphElement>()
                .ToList();

            copies.ForEach(e => ConfigureAndAddElement(e, false));

            if (asCommand)
            {
                RegisterUndoRedoEvent("Paste",
                    onUndo: () => OnElementsDeleted(operation, AskUser.DontAskUser, copies.Cast<ISelectable>().ToList(), false),
                    onRedo: () => Paste(operation, serializedData)
                );
            }
        }

        private class VersionTracker : ScriptableObject
        {
            public int Version;
        }

        private VersionTracker _versionTracker;

        private void InitializeCallbacks()
        {
            _versionTracker = ScriptableObject.CreateInstance<VersionTracker>();
            Undo.undoRedoEvent += OnUndoRedoEvent;

            deleteSelection = (op, askUser) => OnElementsDeleted(op, askUser, selection);
            // elementResized - can be counted as a thing to save, but does not give data to make it a command
            elementsAddedToGroup = (group, elements) => OnElementsAddedToGroup(group, elements);
            // elementsInsertedToStackNode
            elementsRemovedFromGroup = (group, elements) => OnElementsRemovedFromGroup(group, elements);
            // elementsRemovedFromStackNode
            // groupTitleChanged 
            graphViewChanged = (changes) => OnGraphViewChanged(changes);
            serializeGraphElements = (elements) => CopyElements(elements);
            canPasteSerializedData = (data) => ValidatePaste(data);
            unserializeAndPaste = (op, data) => Paste(op, data);
            // viewTransformChanged
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            OnSelectionChanged?.Invoke(selection);
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnSelectionChanged?.Invoke(selection);
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChanged?.Invoke(selection);
        }

        public delegate void SelectionChangedEvent(List<ISelectable> selection);
        public event SelectionChangedEvent OnSelectionChanged;

        private enum GraphState
        {
            New,
            Valid, // There are multiple valid states that allow for compilation and that do not
            Invalid
        }

        private GraphState Validate()
        {
            if (Nodes.Count == 0 && Edges.Count == 0 && Groups.Count == 0)
            {
                return GraphState.New;
            }

            if (Nodes.Where(n => n is BrainNode).Count() != 1)
            {
                return GraphState.Invalid;
            }

            return GraphState.Valid;
        }

        private void Load(AgentGraphData data)
        {
            foreach (AgentGraphGroupData groupData in data.Groups)
            {
                var group = groupData.Load();
                ConfigureAndAddElement(group, false);
            }

            foreach (AgentGraphNodeData nodeData in data.Nodes)
            {
                var node = nodeData.Load();
                node.Draw();

                ConfigureAndAddElement(node, false);
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
                ConfigureAndAddElement(edge, false);
            }
        }

        protected AgentGraphData Asset;

        public AgentGraph Window;

        public AgentGraphView(AgentGraph window, AgentGraphData data)
        {
            Window = window;
            InitializeManipulators();
            InitializeBackground();
            InitializeCallbacks();

            Asset = data;
            Load(Asset);

            switch (Validate())
            {
                case GraphState.New:
                    InitializeDefaultElements();
                    break;
                case GraphState.Invalid:
                    Debug.LogError("Graph's current state is invalid");
                    break;
                default:
                    break;
            }
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

            ApplyRemove();

            return Asset;
        }
    }
}
