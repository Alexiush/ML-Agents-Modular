using System.Linq;
using UnityEditor;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

namespace ModularMLAgents.Utilities
{
    [YamlObject]
    [System.Serializable]
    public partial class NullWhenEmptyString
    {
        public string Value;
    }

    [CustomPropertyDrawer(typeof(NullWhenEmptyString))]
    public class NullWhenEmptyStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customLabel = new GUIContent(property.propertyPath.Split('.').Last());
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Value"), customLabel);
        }
    }

    public class NullWhenEmptyStringFormatter : WriteOnlyYamlFormatter<NullWhenEmptyString>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, NullWhenEmptyString value, YamlSerializationContext context)
        {
            if (value is null || string.IsNullOrEmpty(value.Value))
            {
                emitter.WriteNull();
                return;
            }

            emitter.WriteString(value.Value);
        }
    }
}
