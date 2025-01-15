using ModularMLAgents.Components;
using MLAgents.Editor;
using ModularMLAgents.Utilities;
using ModularMLAgents.Editor;
using ModularMLAgents.Settings;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAgents.Configuration;

public class ConfigurationGeneratorModular : ConfigurationGenerator
{
    [MenuItem("Window/Modular Agents/Configuration Generator")]
    public static new void ShowWindow()
    {
        ConfigurationGenerator wnd = GetWindow<ConfigurationGeneratorModular>();

        wnd.InitializeConfiguration();
        wnd.InitializePath();
        wnd.ParseBehaviors();
        wnd.titleContent = new GUIContent("Configuration generator");
    }

    public override void InitializePath()
    {
        var defaultPath = ModularAgentsSettings.GetOrCreateSettings().DefaultConfigsPath;
        var name = SceneManager.GetActiveScene().name;

        _configPath = System.IO.Path.Combine(defaultPath, $"{name}.yaml");
    }

    private void BindAgentGraphs()
    {
        var behaviorComponents = UnityEngine.Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None)
            .Distinct(new BehaviorsComparer());

        UnityEngine.Object.FindObjectsByType<ModularAgent>(FindObjectsSortMode.None)
            .Select(a => (agent: a, behavior: a.gameObject.GetComponent<BehaviorParameters>()))
            .Where(c => behaviorComponents.Contains(c.behavior) && c.behavior is not null)
            .ToList()
            .ForEach(c => ConfigUtilities.BindBehaviorToAgentGraph(
                _behaviors.First(b => b.BehaviorId == c.behavior.BehaviorName),
                c.agent.GraphData
            ));
    }

    public new void OnGUI()
    {
        if (_serializedObject is null)
        {
            _serializedObject = new SerializedObject(this);
        }
        _serializedObject.Update();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        var behaviorsProperty = _serializedObject.FindProperty("_behaviors");

        _behaviorsOpen = EditorGUILayout.Foldout(
            _behaviorsOpen,
            "Behaviors",
            new GUIStyle(EditorStyles.foldout)
            {
                margin = new RectOffset(2, 0, 0, 0)
            }
        );

        if (_behaviorsOpen)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < behaviorsProperty.arraySize; i++)
            {
                var behavior = behaviorsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(behavior);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_serializedObject.FindProperty("_configuration"));

        _configPath = EditorGUILayout.TextField("Name", _configPath);
        bool _generate = GUILayout.Button("Generate config");
        EditorGUILayout.EndScrollView();

        _serializedObject.ApplyModifiedProperties();
        if (_generate)
        {
            _configuration.Behaviors = _behaviors;
            BindAgentGraphs();
            ConfigCreator.CreateConfig(_configuration, _configPath);
        }
    }
}
