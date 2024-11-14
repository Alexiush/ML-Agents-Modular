using Unity.VisualScripting;
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
    public static void CreateNewGraph()
    {
        AgentGraph wnd = GetWindow<AgentGraph>();
        wnd.OnAssetLoaded();

        wnd.titleContent = new GUIContent("Agent graph");
    }

    public static void OpenGraph(AgentGraphData graphData)
    {
        AgentGraph wnd = GetWindow<AgentGraph>();
        wnd._graphData = graphData;
        wnd.OnAssetLoaded();

        wnd.titleContent = new GUIContent("Agent graph");
    }

    private AgentGraphData _graphData;
    private AgentGraphView _agentGraph;

    private void InitializeGraphView()
    {
        if (_graphData is null)
        {
            _agentGraph = new AgentGraphView();
        }
        else
        {
            _agentGraph = new AgentGraphView(_graphData);
        }
        _agentGraph.StretchToParentSize();

        _document.Add(_agentGraph);
    }

    private void SaveGraph()
    {
        SaveChanges();

        var graphsPath = "Assets\\MLAgentsGraphs";
        SaveUtilities.EnsureFolderExists(graphsPath);
        var graphData = SaveUtilities.GetAsset<AgentGraphData>(graphsPath, Name);

        _graphData = _agentGraph.Save(graphData);

        EditorUtility.SetDirty(_graphData);
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

    private void OnAssetLoaded()
    {
        InitializeGraphView();
        InitializeToolbar();
    }

    public void CreateGUI()
    {
        _document = _visualTreeAsset.Instantiate();
        _document.name = "AgentGraphUXML";
        _document.style.flexGrow = 1;

        rootVisualElement.Add(_document);
    }
}
