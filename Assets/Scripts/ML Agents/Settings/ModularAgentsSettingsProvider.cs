using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace ModularMLAgents.Settings
{
    public class ModularAgentsSettingsProvider : SettingsProvider
    {
        private ModularAgentsSettings _settings;

        private const string customSettingsPath = "Assets/ModularAgents/ModularAgentsSettings.asset";

        public ModularAgentsSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = ModularAgentsSettings.GetOrCreateSettings();
        }

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

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            DrawConfigsPathProperty();
            DrawModelsPathProperty();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new ModularAgentsSettingsProvider("Project/Modular agents", SettingsScope.Project);
        }
    }
}
