using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentGraph : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset _visualTreeAsset = default;
    private VisualElement _document;

    [MenuItem("Assets/Create/Agents/AgentGraph", false, 1)]
    public static void CreateNewGraph()
    {
        var graphData = ScriptableObject.CreateInstance<AgentGraphData>();

        ProjectWindowUtil.CreateAsset(
            graphData,
            "AgentGraph.asset"
        );
    }

    public static void OpenGraph(AgentGraphData graphData)
    {
        AgentGraph wnd = GetWindow<AgentGraph>();
        wnd._graphData = graphData;
        wnd.OnAssetLoaded();

        wnd.titleContent = new GUIContent(graphData.name);
    }

    private AgentGraphData _graphData;
    private AgentGraphView _agentGraph;

    private void InitializeGraphView()
    {
        if (_graphData is null)
        {
            throw new System.ArgumentNullException("No graph data to open");
        }

        _agentGraph = new AgentGraphView(_graphData);
        _agentGraph.StretchToParentSize();

        _document.Add(_agentGraph);
    }

    private void SaveGraph()
    {
        SaveChanges();
        _graphData = _agentGraph.Save(_graphData);
        EditorUtility.SetDirty(_graphData);
        SaveUtilities.SaveAssetsImmediately();
    }

    private void SaveGraphAs()
    {
        var oldPath = AssetDatabase.GetAssetPath(_graphData);
        var directory = System.IO.Path.GetDirectoryName(oldPath);
        
        var newPath = EditorUtility.SaveFilePanel(
           "Save AgentGraph as",
           directory,
           _graphData.name + ".asset",
           "asset");

        if (newPath == string.Empty)
        {
            Debug.LogWarning("Save as was cancelled");
            return;
        }

        if (!newPath.StartsWith(Application.dataPath))
        {
            Debug.LogError("Saving assset out of project");
            return;
        }

        newPath = System.IO.Path.GetRelativePath(Application.dataPath, newPath);
        newPath = System.IO.Path.Combine("Assets", newPath);

        var moveResult = AssetDatabase.MoveAsset(oldPath, newPath);
        
        if (moveResult != string.Empty)
        {
            Debug.LogError(moveResult);
            return;
        }
        
        titleContent = new GUIContent(_graphData.name);
        SaveGraph();
    }

    public string Name { get; private set; } = "AgentGraph";

    private void InitializeToolbar()
    {
        var toolbar = new Toolbar();

        var saveButton = new ToolbarButton()
        {
            text = "Save"
        };
        saveButton.clicked += SaveGraph;
        toolbar.Add(saveButton);

        var saveAsButton = new ToolbarButton()
        {
            text = "Save as..."
        };
        saveAsButton.clicked += SaveGraphAs;
        toolbar.Add(saveAsButton);

        _agentGraph.Add(toolbar);
    }

    private AgentGraphPreviewWindow _agentGraphPreviewWindow;

    private void InitializePreviewWindow()
    {
        _agentGraphPreviewWindow = new AgentGraphPreviewWindow(this, _graphData);
        _agentGraph.Add(_agentGraphPreviewWindow);

        _agentGraph.OnSelectionChanged += selection =>
        {
            if (selection.Count != 1)
            {
                _agentGraphPreviewWindow.UpdateNodeData(null);
                return;
            }

            _agentGraphPreviewWindow.UpdateNodeData(selection.First() as AgentGraphNode);
        };
    }

    private void OnAssetLoaded()
    {
        InitializeGraphView();
        InitializeToolbar();
        InitializePreviewWindow();
    }

    public void CreateGUI()
    {
        _document = _visualTreeAsset.Instantiate();
        _document.name = "AgentGraphUXML";
        _document.style.flexGrow = 1;

        rootVisualElement.Add(_document);
    }
}
