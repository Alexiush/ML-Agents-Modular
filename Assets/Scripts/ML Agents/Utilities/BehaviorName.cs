using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

namespace ModularMLAgents.Utilities
{
    [YamlObject]
    [System.Serializable]
    public partial class BehaviorName
    {
        [YamlIgnore]
        public BehaviorParameters Reference;
        [YamlIgnore]
        public string Value => Reference?.BehaviorName;
    }

    public class BehaviorNameFormatter : WriteOnlyYamlFormatter<BehaviorName>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, BehaviorName value, YamlSerializationContext context)
        {
            context.Serialize(ref emitter, value.Value);
        }
    }

    [InitializeOnLoad]
    public static class BehaviorCache
    {
        public static List<BehaviorParameters> Behaviors { get; private set; } = new List<BehaviorParameters>();

        static BehaviorCache()
        {
            RefreshCache();
            EditorApplication.hierarchyChanged += RefreshCache;
        }

        public static void RefreshCache()
        {
            Behaviors = UnityEngine.Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None)
                .GroupBy(b => b.BehaviorName)
                .Select(g => g.First())
                .ToList();
        }
    }

    [CustomPropertyDrawer(typeof(BehaviorName))]
    public class BehaviorNameDrawer : PropertyDrawer
    {
        private int _selected = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviorCache.Behaviors.Count == 0)
            {
                EditorGUI.LabelField(position, "No behaviors in scene");
                return;
            }

            var valueProperty = property.FindPropertyRelative("Reference");
            _selected = BehaviorCache.Behaviors.IndexOf(valueProperty.boxedValue as BehaviorParameters);
            _selected = _selected == -1 ? 0 : _selected;

            _selected = EditorGUI.Popup(position, property.displayName, _selected, BehaviorCache.Behaviors.Select(b => b.BehaviorName).ToArray());
            valueProperty.boxedValue = BehaviorCache.Behaviors[_selected];
        }
    }
}