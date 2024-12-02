using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ModularMLAgents.Models;
using Unity.Sentis;

namespace ModularMLAgents
{
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

        public abstract void ApplyValidationStyle(ValidationReport validationReport);
        public abstract ValidationReport Validate(); 
    }

    public abstract class AgentGraphNode : Node, IAgentGraphNode, IAgentGraphElement
    {
        protected AgentGraphNodeData Data;
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
        public virtual void Draw()
        {
            titleContainer.Q<Label>("title-label").text = this.GetType().Name;

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

        public abstract IAgentGraphElement Copy();

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

        public virtual void ApplyValidationStyle(ValidationReport validationReport) { }

        public virtual ValidationReport Validate() => new ValidationReport();
    }
}
