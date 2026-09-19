using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RequiredAttribute))]
public class RequiredAttributeDrawer : PropertyDrawer
{
    private static readonly Color EmptyFieldColor = new Color(1f, 0.35f, 0.35f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool isEmpty = property.propertyType == SerializedPropertyType.ObjectReference
                       && property.objectReferenceValue == null;

        Color previous = GUI.color;

        if (isEmpty)
            GUI.color = EmptyFieldColor;

        EditorGUI.PropertyField(position, property, label);
        GUI.color = previous;
    }
}
