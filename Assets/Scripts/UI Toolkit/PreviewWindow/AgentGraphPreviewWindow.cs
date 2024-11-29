using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using ModularMLAgents.Trainers;
using ModularMLAgents.Models;
using UnityEngine;

namespace ModularMLAgents.Editor
{
    public class AgentGraphPreviewWindow : VisualElement
    {
        private VisualElement _nodeDataTab;

        public void UpdateNodeData(AgentGraphNode agentGraphNode)
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

        private void InitializeTabs()
        {
            var nodeDataTab = new Button()
            {
                text = "Node data",
                name = "NodeTab"
            };
            nodeDataTab.AddToClassList("tab");

            var graphDataTab = new Button()
            {
                text = "Graph data",
                name = "GraphTab"
            };
            graphDataTab.AddToClassList("tab");

            var tabContainer = new VisualElement();
            tabContainer.name = "tabs";
            tabContainer.Add(nodeDataTab);
            tabContainer.Add(graphDataTab);

            Add(tabContainer);
        }

        private void InitializeContent(AgentGraphData graphData)
        {
            var nodeDataContent = new VisualElement()
            {
                name = "NodeContent"
            };
            nodeDataContent.AddToClassList("unselectedContent");
            _nodeDataTab = nodeDataContent;

            var graphDataContent = new VisualElement()
            {
                name = "GraphContent"
            };
            graphDataContent.AddToClassList("currentlySelectedContent");
            _graphDataTab = graphDataContent;
            InitializeGraphTab(graphData);

            var contentContainer = new ScrollView();
            // Somehow it overlaps with ContentZoomer
            contentContainer.RegisterCallback<WheelEvent>(evt => evt.StopImmediatePropagation());

            contentContainer.name = "tabContent";
            contentContainer.Add(nodeDataContent);
            contentContainer.Add(graphDataContent);

            contentContainer.style.flexGrow = 1;
            Add(contentContainer);
        }

        private TabbedMenuController controller;

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

            InitializeTabs();
            InitializeContent(graphData);

            controller = new TabbedMenuController(this);
            controller.RegisterTabCallbacks();
        }
    }

    public class TabbedMenuController
    {
        /* Define member variables*/
        private const string tabClassName = "tab";
        private const string currentlySelectedTabClassName = "currentlySelectedContent";
        private const string unselectedContentClassName = "unselectedContent";
        // Tab and tab content have the same prefix but different suffix
        // Define the suffix of the tab name
        private const string tabNameSuffix = "Tab";
        // Define the suffix of the tab content name
        private const string contentNameSuffix = "Content";

        private readonly VisualElement root;

        public TabbedMenuController(VisualElement root)
        {
            this.root = root;
        }

        public void RegisterTabCallbacks()
        {
            UQueryBuilder<Button> tabs = GetAllTabs();
            tabs.ForEach((Button tab) =>
            {
                tab.RegisterCallback<ClickEvent>(TabOnClick);
            });
        }

        /* Method for the tab on-click event: 

           - If it is not selected, find other tabs that are selected, unselect them 
           - Then select the tab that was clicked on
        */
        private void TabOnClick(ClickEvent evt)
        {
            Button clickedTab = evt.currentTarget as Button;
            if (!TabIsCurrentlySelected(clickedTab))
            {
                GetAllTabs().Where(
                    (tab) => tab != clickedTab && TabIsCurrentlySelected(tab)
                ).ForEach(UnselectTab);
                SelectTab(clickedTab);
            }
        }
        //Method that returns a Boolean indicating whether a tab is currently selected
        private static bool TabIsCurrentlySelected(Button tab)
        {
            return tab.ClassListContains(currentlySelectedTabClassName);
        }

        private UQueryBuilder<Button> GetAllTabs()
        {
            return root.Query<Button>(className: tabClassName);
        }

        /* Method for the selected tab: 
           -  Takes a tab as a parameter and adds the currentlySelectedTab class
           -  Then finds the tab content and removes the unselectedContent class */
        private void SelectTab(Button tab)
        {
            tab.AddToClassList(currentlySelectedTabClassName);
            VisualElement content = FindContent(tab);
            content.RemoveFromClassList(unselectedContentClassName);
        }

        /* Method for the unselected tab: 
           -  Takes a tab as a parameter and removes the currentlySelectedTab class
           -  Then finds the tab content and adds the unselectedContent class */
        private void UnselectTab(Button tab)
        {
            tab.RemoveFromClassList(currentlySelectedTabClassName);
            VisualElement content = FindContent(tab);
            content.AddToClassList(unselectedContentClassName);
        }

        // Method to generate the associated tab content name by for the given tab name
        private static string GenerateContentName(Button tab) =>
            tab.name.Replace(tabNameSuffix, contentNameSuffix);

        // Method that takes a tab as a parameter and returns the associated content element
        private VisualElement FindContent(Button tab)
        {
            return root.Q(GenerateContentName(tab));
        }
    }
}
