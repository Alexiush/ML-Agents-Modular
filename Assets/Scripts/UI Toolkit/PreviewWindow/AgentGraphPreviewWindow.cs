using ModularMLAgents.Models;
using ModularMLAgents.Trainers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModularMLAgents.Editor
{
    public class AgentGraphPreviewWindow : VisualElement
    {
        private VisualElement _nodeDataTab;

        public void UpdateNodeData(IAgentGraphNode agentGraphNode)
        {
            if (agentGraphNode is null)
            {
                _nodeDataTab.Clear();
                return;
            }

            agentGraphNode.DrawParameters(_nodeDataTab);
        }

        private VisualElement _graphDataTab;

        private void InitializeGraphTab(AgentGraphData graphData)
        {
            SerializedObject serializedObject = new SerializedObject(graphData);

            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    var propertyType = typeof(AgentGraphData).GetField(property.propertyPath)?.FieldType;
                    if (propertyType != typeof(ITrainer))
                    {
                        continue;
                    }

                    PropertyField propertyField = new PropertyField(property);
                    propertyField.Bind(serializedObject);

                    _graphDataTab.Add(propertyField);
                }
                while (property.NextVisible(false));
            }
        }

        private void InitializeContent(AgentGraphData graphData)
        {
            var nodeDataContent = new VisualElement()
            {
                name = "NodeContent"
            };
            nodeDataContent.AddToClassList("currentlySelectedContent");
            _nodeDataTab = nodeDataContent;

            var contentContainer = new ScrollView();
            contentContainer.RegisterCallback<WheelEvent>(evt => evt.StopImmediatePropagation());

            contentContainer.name = "tabContent";
            contentContainer.Add(nodeDataContent);

            contentContainer.style.flexGrow = 1;
            Add(contentContainer);
        }

        public AgentGraphPreviewWindow(AgentGraph graph, AgentGraphData graphData)
        {
            AddToClassList("PreviewWindow");

            style.position = Position.Absolute;
            style.bottom = 30;
            style.right = 30;

            style.minHeight = 100;
            style.maxHeight = style.height = Length.Percent(25);

            style.minWidth = 125;
            style.maxWidth = style.width = Length.Percent(30);

            style.backgroundColor = new StyleColor(new Color32(56, 56, 56, 255));

            InitializeContent(graphData);
        }
    }
}
