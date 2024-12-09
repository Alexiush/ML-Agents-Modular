using ModularMLAgents;
using ModularMLAgents.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class ConfigurationGenerator : EditorWindow
{
    [MenuItem("Window/ModularAgents/ConfigurationGenerator")]
    public static void ShowWindow()
    {
        ConfigurationGenerator wnd = GetWindow<ConfigurationGenerator>();

        wnd.InitializeConfiguration();
        wnd.InitializePath();
        wnd.ParseBehaviors();
        wnd.titleContent = new GUIContent("Configuration generator");
    }

    private string _configPath;

    public void InitializePath()
    {
        var defaultPath = ModularAgentsSettings.GetOrCreateSettings().DefaultConfigsPath;
        var name = SceneManager.GetActiveScene().name;

        _configPath = System.IO.Path.Combine(defaultPath, $"{name}.yaml");
    }

    private class CandidatesEqualityComparer : IEqualityComparer<(ModularAgent agent, BehaviorParameters behavior)>
    {
        public bool Equals((ModularAgent agent, BehaviorParameters behavior) b1, (ModularAgent agent, BehaviorParameters behavior) b2)
        {
            if (ReferenceEquals(b1, b2))
                return true;

            return b1.behavior.BehaviorName == b2.behavior.BehaviorName;
        }

        public int GetHashCode((ModularAgent agent, BehaviorParameters behavior) candidate) => candidate.behavior.BehaviorName.GetHashCode();
    }

    [SerializeField]
    private List<string> _behaviors;

    public void ParseBehaviors()
    {
        _configuration.Behaviors = UnityEngine.Object.FindObjectsOfType<ModularAgent>()
            .Select(a => (agent: a, behavior: a.gameObject.GetComponent<BehaviorParameters>()))
            .Where(c => c.behavior is not null)
            .Distinct(new CandidatesEqualityComparer())
            .Select(c => ConfigUtilities.CreateBehavior(c.agent.GraphData, c.behavior.BehaviorName))
            .ToList();

        _behaviors = _configuration.Behaviors.Select(b => b.BehaviorId).ToList();
    }

    private SerializedObject _serializedObject;
    [SerializeField]
    private Configuration _configuration;

    public void InitializeConfiguration()
    {
        _configuration = new Configuration();
    }

    private Vector2 _scrollPosition;

    public void OnGUI()
    {
        _serializedObject = new SerializedObject(this);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUI.BeginDisabledGroup(true);
        var behaviorsProperty = _serializedObject.FindProperty("_behaviors");
        EditorGUILayout.PropertyField(behaviorsProperty);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(_serializedObject.FindProperty("_configuration"));

        _configPath = EditorGUILayout.TextField("Name", _configPath);
        bool _generate = GUILayout.Button("Generate config");
        EditorGUILayout.EndScrollView();

        _serializedObject.ApplyModifiedProperties();
        if (_generate)
        {
            ConfigUtilities.CreateConfig(_configuration, _configPath);
        }
    }
}
