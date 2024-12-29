using ModularMLAgents.Compilation;
using ModularMLAgents.Models;
using ModularMLAgents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularMLAgents
{
    public interface IAgentGraphNode : IAgentGraphElement
    {
        public Node Node { get; }
        public List<Port> Ports { get; set; }

        public void DrawParameters(VisualElement canvas);
        public void Draw(AgentGraphContext context);
        public abstract AgentGraphNodeData Save(UnityEngine.Object parent);
    }

    public interface IAgentGraphElement
    {
        public abstract UnityEngine.Object GetData();
        public abstract AgentGraphElementMetadata GetMetadata();
        public abstract GraphElement GetParentComposite();
        public abstract void SetParentComposite(GraphElement parentComposite);
        public abstract IAgentGraphElement Copy(AgentGraphContext context);

        public abstract void ApplyValidationStyle(ValidationReport validationReport);
        public abstract ValidationReport Validate(ValidationReport validationReport);
    }

    public abstract class AgentGraphNodeBase<T> : Node, IAgentGraphNode
        where T : AgentGraphNodeData
    {
        public Node Node => this;

        protected AgentGraphNodeData Data;
        protected AgentGraphNodeData RuntimeData;

        public List<Port> Ports { get; set; } = new List<Port>();
        public string Name => RuntimeData.name;

        protected AgentGraphContext Context;

        public AgentGraphNodeBase(AgentGraphContext context, T data = null)
        {
            Context = context;
        }

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

        private (HashSet<string> trackedPaths, Dictionary<string, string> subclassSelectors) GetTrackedPaths()
        {
            HashSet<string> trackedPaths = new HashSet<string>();
            Dictionary<string, string> subclassSelectors = new Dictionary<string, string>();

            var serializedObject = new SerializedObject(RuntimeData);
            var serializedProperty = serializedObject.GetIterator();

            BindingFlags AllBindingFlags = (BindingFlags)(-1);
            while (serializedProperty.Next(true))
            {
                var targetObjectType = RuntimeData.GetType();
                if (targetObjectType == null)
                {
                    continue;
                }

                var path = serializedProperty.propertyPath;

                foreach (var pathSegment in serializedProperty.propertyPath.Split('.').SkipLast(1))
                {
                    if (targetObjectType == null)
                    {
                        break;
                    }

                    var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
                    var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);

                    switch (fieldInfo, propertyInfo)
                    {
                        case (null, null):
                            targetObjectType = null;
                            break;
                        case (FieldInfo field, _):
                            targetObjectType = field.FieldType;
                            break;
                        case (_, PropertyInfo prop):
                            targetObjectType = prop.PropertyType;
                            break;
                    }
                }

                if (targetObjectType == null)
                {
                    continue;
                }

                var actualFieldInfo = targetObjectType.GetField(serializedProperty.name, AllBindingFlags);
                var actualPropertyInfo = targetObjectType.GetProperty(serializedProperty.name, AllBindingFlags);
                ValidationObservedAttribute[] validationObservedAttributes = new ValidationObservedAttribute[] { };
                SubclassSelectorAttribute[] subclassSelectorAttributes = new SubclassSelectorAttribute[] { };

                switch (actualFieldInfo, actualPropertyInfo)
                {
                    case (null, null):
                        break;
                    case (FieldInfo field, _):
                        validationObservedAttributes = (ValidationObservedAttribute[])field.GetCustomAttributes<ValidationObservedAttribute>(false);
                        subclassSelectorAttributes = (SubclassSelectorAttribute[])field.GetCustomAttributes<SubclassSelectorAttribute>(false);
                        break;
                    case (_, PropertyInfo prop):
                        validationObservedAttributes = (ValidationObservedAttribute[])prop.GetCustomAttributes<ValidationObservedAttribute>(false);
                        subclassSelectorAttributes = (SubclassSelectorAttribute[])prop.GetCustomAttributes<SubclassSelectorAttribute>(false);
                        break;
                }

                if (validationObservedAttributes.Length > 0)
                {
                    trackedPaths.Add(path);
                }

                if (subclassSelectorAttributes.Length > 0
                    && trackedPaths.Any(trackedPath => path.StartsWith(trackedPath))
                    )
                {
                    subclassSelectors.Add(path, serializedProperty.managedReferenceFullTypename);
                }
            }

            return (trackedPaths, subclassSelectors);
        }

        private SerializedObject _serializedObject;

        public virtual void DrawParameters(VisualElement canvas)
        {
            _serializedObject = new SerializedObject(RuntimeData);
            SerializedProperty property = _serializedObject.GetIterator();

            bool boundTopProperty = false;
            if (property.NextVisible(true))
            {
                do
                {
                    PropertyField propertyField = new PropertyField(property);
                    propertyField.Bind(_serializedObject);
                    if (!boundTopProperty)
                    {
                        propertyField.TrackSerializedObjectValue(_serializedObject,
                            o => Context.UpdateChangesStatus(RuntimeData, true)
                        );
                        boundTopProperty = true;
                    }

                    canvas.Add(propertyField);
                }
                while (property.NextVisible(false));
            }

            void ValidateOnChange(SerializedPropertyChangeEvent callback)
            {
                Validate(new ValidationReport());
            }

            var callback = new EventCallback<SerializedPropertyChangeEvent>(ValidateOnChange);
            var (trackedPaths, subclassSelectors) = GetTrackedPaths();
            HashSet<PropertyField> trackedPropertyFields = new HashSet<PropertyField>();

            canvas.RegisterCallback<GeometryChangedEvent>(e =>
            {
                if (subclassSelectors.Any(kv => kv.Value != _serializedObject.FindProperty(kv.Key)?.managedReferenceFullTypename))
                {
                    Validate(new ValidationReport());
                }

                subclassSelectors.Keys.ToList()
                    .ForEach(key => subclassSelectors[key] = _serializedObject.FindProperty(key)?.managedReferenceFullTypename);

                var propertyFields = canvas.Query()
                    .Descendents<PropertyField>()
                    .ToList();

                trackedPropertyFields.Except(propertyFields)
                    .ToList()
                    .ForEach(p => p.UnregisterCallback<SerializedPropertyChangeEvent>(callback));

                propertyFields.Except(trackedPropertyFields)
                    .Where(p => trackedPaths.Contains(p.bindingPath))
                    .ToList()
                    .ForEach(p =>
                    {
                        trackedPropertyFields.Add(p);
                        p.RegisterCallback<SerializedPropertyChangeEvent>(callback);
                    });
            });
        }

        public virtual void Draw(AgentGraphContext context)
        {
            var nodeNameField = new TextField
            {
                value = Name
            };

            nodeNameField.RegisterCallback<BlurEvent>((evt) => context.Rename(RuntimeData, nodeNameField.value));
            nodeNameField.RegisterCallback<KeyDownEvent>((evt) =>
            {

                if (evt.keyCode == KeyCode.Return || evt.character == '\n')
                {
                    // Submit logic
                    context.Rename(RuntimeData, nodeNameField.value);
                }

                if (evt.keyCode == KeyCode.Tab || (evt.modifiers == EventModifiers.Shift && (evt.keyCode == KeyCode.Tab || evt.character == '\t')))
                {
                    // Focus logic
                    context.Rename(RuntimeData, nodeNameField.value);
                }

            }, TrickleDown.TrickleDown);
            context.SubscribeToNameChanges(RuntimeData, newName => nodeNameField.SetValueWithoutNotify(newName));

            titleContainer.Insert(0, nodeNameField);
            titleContainer.Q<Label>("title-label").style.display = DisplayStyle.None; //.text = Name;

            var validationData = new Label { name = "validation-data" };
            mainContainer.Insert(1, validationData);

            foreach (var port in Ports.Where(p => p.direction == Direction.Input))
            {
                inputContainer.Add(port);
            }

            foreach (var port in Ports.Where(p => p.direction == Direction.Output))
            {
                outputContainer.Add(port);
            }

            VisualElement container = new VisualElement();
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;

            DrawParameters(container);

            this.extensionContainer.Add(container);
            RefreshExpandedState();
        }

        public virtual UnityEngine.Object GetData() => RuntimeData;

        protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
        public virtual AgentGraphElementMetadata GetMetadata() => Metadata;

        public override void SetPosition(Rect newPos)
        {
            Metadata.Position = newPos;
            base.SetPosition(newPos);
        }

        public virtual AgentGraphNodeData Save(UnityEngine.Object parent)
        {
            if (!AssetDatabase.Contains(Data))
            {
                AssetDatabase.AddObjectToAsset(Data, parent);
            }

            EditorUtility.CopySerializedIfDifferent(RuntimeData, Data);
            Data.Metadata = Metadata;
            Data.Ports = Ports.Select(p => new AgentGraphPortData(p)).ToList();

            return Data;
        }

        public virtual IAgentGraphElement Copy(AgentGraphContext context)
        {
            var copyData = context.CreateInstance<T>(RuntimeData.name);
            copyData.Metadata = Metadata;
            copyData.Metadata.GUID = Guid.NewGuid().ToString();

            var node = copyData.Load(context);
            Ports.ForEach(p => node.Node.InstantiatePort(p.orientation, p.direction, p.capacity, p.portType));
            node.Draw(context);

            return node;
        }

        public virtual void ApplyValidationStyle(ValidationReport validationReport)
        {
            contentContainer.Q<Label>("validation-data").text = string.Join("\n", validationReport.Errors);
            mainContainer.EnableInClassList("Invalid", !validationReport.Valid);
        }

        protected virtual List<DynamicTensorShape> GetInputShape()
        {
            return Context.GetInputNodes(RuntimeData)
                .SelectMany(n => n.GetPartialOutputShape(Context, RuntimeData))
                .ToList();
        }

        protected virtual void PropagateValidation()
        {
            Ports.Where(p => p.direction == Direction.Output)
                .SelectMany(p => p.connections)
                .Select(e => e.input.node)
                .OfType<IAgentGraphNode>()
                .ToList()
                .ForEach(n => n.Validate(new ValidationReport()));

            if (this is IShapeRequestor)
            {
                Ports.Where(p => p.direction == Direction.Input)
                .SelectMany(p => p.connections)
                .Select(e => e.output.node)
                .OfType<IAgentGraphNode>()
                .ToList()
                .ForEach(n => n.Validate(new ValidationReport()));
            }
        }

        public virtual ValidationReport Validate(ValidationReport validationReport)
        {
            if (validationReport.Valid)
            {
                PropagateValidation();
            }

            ApplyValidationStyle(validationReport);
            return validationReport;
        }
    }

    public class AgentGraphGroup : Group, IAgentGraphElement
    {
        protected AgentGraphGroupData Data;
        public virtual UnityEngine.Object GetData() => Data;

        protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
        public virtual AgentGraphElementMetadata GetMetadata() => Metadata;
        public AgentGraphGroup(AgentGraphContext context) : base()
        {
            Data = context.CreateInstance<AgentGraphGroupData>("New group");
            title = Data.name;
        }

        public AgentGraphGroup(AgentGraphContext context, AgentGraphGroupData data) : base()
        {
            Metadata = data.Metadata;
            viewDataKey = Metadata.GUID;


            Data = data;
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

        public List<IAgentGraphNode> Nodes = new List<IAgentGraphNode>();
        public List<AgentGraphGroup> Groups = new List<AgentGraphGroup>();

        public AgentGraphGroupData Save(UnityEngine.Object rootAsset, AgentGraphContext context)
        {
            if (!AssetDatabase.Contains(Data))
            {
                AssetDatabase.AddObjectToAsset(Data, rootAsset);
            }

            Data.Groups = Groups
                .Select(g => g.Save(Data, context))
                .ToList();

            Data.Nodes = Nodes
                .Select(n => n.Save(Data))
                .ToList();

            Data.Metadata = Metadata;
            return Data;
        }

        public IAgentGraphElement Copy(AgentGraphContext context)
        {
            var copyData = context.CreateInstance<AgentGraphGroupData>(Data.name);
            copyData.Metadata = Metadata;
            copyData.Metadata.GUID = Guid.NewGuid().ToString();

            return new AgentGraphGroup(context, copyData);
        }

        public virtual void ApplyValidationStyle(ValidationReport validationReport) { }

        public virtual ValidationReport Validate(ValidationReport validationReport) => validationReport;
    }
}
