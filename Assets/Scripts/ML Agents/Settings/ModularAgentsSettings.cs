using ModularMLAgents.Saving;
using UnityEditor;
using UnityEngine;

namespace ModularMLAgents.Settings
{
    public class ModularAgentsSettings : ScriptableObject
    {
        [field: HideInInspector]
        [field: SerializeField]
        public string DefaultConfigsPath { get; private set; } = "Assets/ModularAgents/Configs";
        public void UpdateConfigsPath(string newPath)
        {
            DefaultConfigsPath = newPath;
            EditorUtility.SetDirty(this);
        }

        [field: HideInInspector]
        [field: SerializeField]
        public string DefaultModelsPath { get; private set; } = "Assets/ModularAgents/Models";
        public void UpdateModelsPath(string newPath)
        {
            DefaultModelsPath = newPath;
            EditorUtility.SetDirty(this);
        }

        public const string SettingsDirectory = "ModularAgents";
        public const string SettingsFile = "ModularAgentsSettings.asset";
        public static string SettingsPath => System.IO.Path.Combine("Assets", SettingsDirectory, SettingsFile);

        public static ModularAgentsSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ModularAgentsSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ModularAgentsSettings>();
                SaveUtilities.EnsureFolderExists(SettingsDirectory);
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    [CustomEditor(typeof(ModularAgentsSettings))]
    public class ModularAgentsSettingsEditor : UnityEditor.Editor
    {
        private ModularAgentsSettings _settings;

        private void DrawConfigsPathProperty()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Default configs path", EditorStyles.label, GUILayout.Width(EditorGUIUtility.labelWidth - 1),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUI.BeginDisabledGroup(true);
            var path = EditorGUILayout.TextField(_settings.DefaultConfigsPath);
            EditorGUI.EndDisabledGroup();

            var clicked = GUILayout.Button("Browse...", GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();

            if (clicked)
            {
                string newPath = EditorUtility.SaveFolderPanel(
                   "Select folder",
                   System.IO.Path.Combine("Assets", _settings.DefaultConfigsPath),
                   "Configs"
                );

                if (string.IsNullOrEmpty(newPath))
                {
                    return;
                }

                _settings.UpdateConfigsPath(newPath);
                path = newPath;
            }
        }

        private void DrawModelsPathProperty()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Default models path", EditorStyles.label, GUILayout.Width(EditorGUIUtility.labelWidth - 1),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUI.BeginDisabledGroup(true);
            var path = EditorGUILayout.TextField(_settings.DefaultModelsPath);
            EditorGUI.EndDisabledGroup();

            var clicked = GUILayout.Button("Browse...", GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();

            if (clicked)
            {
                string newPath = EditorUtility.SaveFolderPanel(
                   "Select folder",
                   System.IO.Path.Combine("Assets", _settings.DefaultModelsPath),
                   "Models"
                );

                if (string.IsNullOrEmpty(newPath))
                {
                    return;
                }

                _settings.UpdateModelsPath(newPath);
                path = newPath;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _settings = (ModularAgentsSettings)target;

            DrawConfigsPathProperty();
            DrawModelsPathProperty();
        }
    }
}
