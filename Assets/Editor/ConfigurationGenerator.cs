using ModularMLAgents.Configuration;
using ModularMLAgents.Settings;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModularMLAgents.Editor
{
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

        public class BehaviorsComparer : IEqualityComparer<BehaviorParameters>
        {
            public bool Equals(BehaviorParameters first, BehaviorParameters second)
            {
                if (ReferenceEquals(first, second))
                    return true;

                return first.BehaviorName == second.BehaviorName;
            }

            public int GetHashCode(BehaviorParameters behavior) => behavior.BehaviorName.GetHashCode();
        }

        private List<string> _managedSerializedProperties = new List<string>();

        [SerializeField]
        private List<Behavior> _behaviors;

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

        public void ParseBehaviors()
        {
            var behaviorComponents = UnityEngine.Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None)
                .Distinct(new BehaviorsComparer());

            _behaviors = behaviorComponents
                .Select(b => new Behavior { BehaviorId = b.BehaviorName })
                .ToList();

            // Bind behavior presets
            UnityEngine.Object.FindObjectsOfType<BehaviorConfig>()
                .GroupBy(c => c.BehaviorName)
                .Select(c => c.First())
                .ToList()
                .ForEach(c =>
                {
                    var behaviorIndex = _behaviors.FindIndex(b => b.BehaviorId == c.BehaviorName.Value);

                    if (behaviorIndex == -1)
                    {
                        return;
                    }

                    c.Behavior.BehaviorId = c.BehaviorName.Value;
                    _behaviors[behaviorIndex] = c.Behavior;

                    _managedSerializedProperties.Add($"_behaviors.data[{behaviorIndex}]");
                });
        }

        private SerializedObject _serializedObject;
        [SerializeField]
        private Config _configuration;

        public void InitializeConfiguration()
        {
            var config = UnityEngine.Object.FindObjectsByType<EnvironmentConfig>(FindObjectsSortMode.None)
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (config != null)
            {
                _configuration = config.Configuration;
                _managedSerializedProperties.Add("_configuration");

                return;
            }

            _configuration = new Config();
        }

        private Vector2 _scrollPosition;
        private bool _behaviorsOpen;

        public void OnGUI()
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
                ConfigUtilities.CreateConfig(_configuration, _configPath);
            }
        }
    }
}
