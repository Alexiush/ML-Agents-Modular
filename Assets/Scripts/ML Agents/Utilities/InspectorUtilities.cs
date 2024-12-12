using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ModularMLAgents.Utilities
{
    public static class InspectorUtilities
    {
        public static void DrawFilteredProperties<T>(T inspectedObject, System.Predicate<SerializedProperty> filter, VisualElement canvas) where T : UnityEngine.Object
        {
            SerializedObject serializedObject = new SerializedObject(inspectedObject);
            SerializedProperty property = serializedObject.GetIterator();

            if (property.NextVisible(true))
            {
                do
                {
                    if (!filter(property))
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
