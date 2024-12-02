using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ModularMLAgents.Utilities
{
    public static class InspectorUtilities
    {
        public static void DrawFilteredProperties<T>(T inspectedObject, System.Predicate<FieldInfo> filter, VisualElement canvas) where T : UnityEngine.Object
        {
            SerializedObject serializedObject = new SerializedObject(inspectedObject);
            var dynamicType = inspectedObject.GetType();

            SerializedProperty property = serializedObject.GetIterator();

            if (property.NextVisible(true))
            {
                do
                {
                    var field = dynamicType.GetField(property.propertyPath);
                    if (!filter(field))
                    {
                        continue;
                    }

                    PropertyField propertyField = new PropertyField(property);
                    propertyField.Bind(serializedObject);

                    canvas.Add(propertyField);
                }
                while (property.NextVisible(false));
            }
        }
    }
}
