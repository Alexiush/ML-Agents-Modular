using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentGraph : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset _visualTreeAsset = default;
    private VisualElement _document;

    [MenuItem("Agents/AgentGraph")]
    public static void ShowExample()
    {
        AgentGraph wnd = GetWindow<AgentGraph>();
        wnd.titleContent = new GUIContent("Agent graph");
    }

    private AgentGraphView _agentGraph;

    private void InitializeGraphView()
    {
        var graphsPath = "Assets\\MLAgentsGraphs";
        SaveUtilities.EnsureFolderExists(graphsPath);
        var graphData = SaveUtilities.GetAsset<AgentGraphData>(graphsPath, Name);

        _agentGraph = new AgentGraphView(graphData);
        _agentGraph.StretchToParentSize();

        _document.Add(_agentGraph);
    }

    private void SaveGraph()
    {
        SaveChanges();

        var graphsPath = "Assets\\MLAgentsGraphs";
        SaveUtilities.EnsureFolderExists(graphsPath);
        var graphData = SaveUtilities.GetAsset<AgentGraphData>(graphsPath, Name);

        _agentGraph.Save(graphData);

        EditorUtility.SetDirty(graphData);
        SaveUtilities.SaveAssetsImmediately();
    }

    public string Name { get; private set; } = "AgentGraph";

    private void InitializeToolbar()
    {
        var toolbar = new Toolbar();

        var label = new Label("Filename:");
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        toolbar.Add(label);

        var textField = new TextField
        {
            value = Name,
        };
        toolbar.Add(textField);
        textField.RegisterValueChangedCallback(e => Name = e.newValue);

        var saveButton = new Button()
        {
            text = "Save"
        };
        saveButton.clicked += SaveGraph;
        toolbar.Add(saveButton);

        _agentGraph.Add(toolbar);
    }

    public void CreateGUI()
    {
        _document = _visualTreeAsset.Instantiate();
        _document.name = "AgentGraphUXML";
        _document.style.flexGrow = 1;

        rootVisualElement.Add(_document);
        InitializeGraphView();
        InitializeToolbar();
    }
}
