using UnityEditor;
using UnityEditor.UIElements;
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
        _agentGraph = new AgentGraphView();
        _agentGraph.StretchToParentSize();

        _document.Add(_agentGraph);
    }

    private void SaveGraph(string path)
    {
        _agentGraph.Save(path);
        SaveChanges();
    }

    private void InitializeToolbar()
    {
        var toolbar = new Toolbar();

        var label = new Label("Filename:");
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        toolbar.Add(label);

        var textField = new TextField
        {
            value = "AgentGraph.asset"
        };
        toolbar.Add(textField);

        var saveButton = new Button()
        {
            text = "Save"
        };
        void SaveClosure() => SaveGraph(textField.value);
        saveButton.clicked += SaveClosure;
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
