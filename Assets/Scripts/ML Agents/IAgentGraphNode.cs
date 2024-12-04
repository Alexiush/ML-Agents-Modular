using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Utilities;
using ModularMLAgents.Models;
using Unity.Sentis;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace ModularMLAgents
{
    public interface IAgentGraphNode
    {
        public void Draw(AgentGraphContext context);
    }

    public interface IAgentGraphElement
    {
        public abstract AgentGraphElementMetadata GetMetadata();
        public abstract GraphElement GetParentComposite();
        public abstract void SetParentComposite(GraphElement parentComposite);
        public abstract IAgentGraphElement Copy(AgentGraphContext context);

        public abstract void ApplyValidationStyle(ValidationReport validationReport);
        public abstract ValidationReport Validate(); 
    }

    public abstract class AgentGraphNode : Node, IAgentGraphNode, IAgentGraphElement
    {
        protected AgentGraphNodeData Data;
        public List<Port> Ports = new List<Port>();
        public string Name => Data.name;

        public AgentGraphNode(AgentGraphContext context) { }

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
        public virtual void Draw(AgentGraphContext context)
        {
            var nodeNameField = new TextField
            {
                value = Name
            };

            nodeNameField.RegisterCallback<BlurEvent>((evt) => context.Rename(Data, nodeNameField.value));
            nodeNameField.RegisterCallback<KeyDownEvent>((evt) => {

                if (evt.keyCode == KeyCode.Return || evt.character == '\n')
                {
                    // Submit logic
                    context.Rename(Data, nodeNameField.value);
                }

                if (evt.keyCode == KeyCode.Tab || (evt.modifiers == EventModifiers.Shift && (evt.keyCode == KeyCode.Tab || evt.character == '\t')))
                {
                    // Focus logic
                    context.Rename(Data, nodeNameField.value);
                }

            }, TrickleDown.TrickleDown);
            context.SubscribeToNameChanges(Data, newName => nodeNameField.SetValueWithoutNotify(newName));

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

            Data.Metadata = Metadata;
            Data.Ports = Ports.Select(p => new AgentGraphPortData(p)).ToList();

            return Data;
        }

        public abstract IAgentGraphElement Copy(AgentGraphContext context);

        public virtual void ApplyValidationStyle(ValidationReport validationReport)
        {
            contentContainer.Q<Label>("validation-data").text = string.Join("\n", validationReport.Errors);
            mainContainer.EnableInClassList("Invalid", !validationReport.Valid);
        }

        public abstract ValidationReport Validate();

        public abstract List<TensorShape> GetOutputShapes();
    }

    public class AgentGraphGroup : Group, IAgentGraphElement
    {
        protected AgentGraphGroupData Data;
        protected AgentGraphElementMetadata Metadata = new AgentGraphElementMetadata();
        public virtual AgentGraphElementMetadata GetMetadata() => Metadata;
        public AgentGraphGroup(AgentGraphContext context) : base()
        { 
            Data = context.CreateInstance<AgentGraphGroupData>("New group");
            Metadata.Asset = Data;

            title = Data.name;
        }

        public AgentGraphGroup(AgentGraphContext context, AgentGraphElementMetadata metadata) : base()
        {
            viewDataKey = metadata.GUID;
            Metadata = metadata;

            Data = metadata.Asset as AgentGraphGroupData;
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
            var copyMetadata = Metadata;
            copyMetadata.Asset = context.CreateInstance<AgentGraphGroupData>(Metadata.Asset.name);
            copyMetadata.GUID = Guid.NewGuid().ToString();

            return new AgentGraphGroup(context, copyMetadata);
        }

        public virtual void ApplyValidationStyle(ValidationReport validationReport) { }

        public virtual ValidationReport Validate() => new ValidationReport();
    }
}
