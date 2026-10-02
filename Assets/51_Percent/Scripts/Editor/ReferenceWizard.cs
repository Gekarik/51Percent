using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ReferenceWizard : EditorWindow
{
    private static readonly Color ProblemColor = new Color(1f, 0.36f, 0.36f);
    private static readonly Color ResolvedColor = new Color(0.45f, 0.85f, 0.45f);

    private List<ReferenceValidator.Violation> _violations = new List<ReferenceValidator.Violation>();
    private Vector2 _scroll;

    [MenuItem("Tools/51 Percent/Reference Wizard")]
    public static void Open()
    {
        var window = GetWindow<ReferenceWizard>("Reference Wizard");
        window.Rescan();
        window.Repaint();
    }

    private void OnEnable()
    {
        Rescan();
    }

    private void Rescan()
    {
        _violations = ReferenceValidator.FindSceneViolations();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Сканировать сцену"))
            Rescan();

        if (_violations.Count == 0)
        {
            EditorGUILayout.HelpBox("Проблемных ссылок нет.", MessageType.Info);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var violation in _violations)
        {
            if (violation.Component == null)
                continue;

            DrawViolationRow(violation);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawViolationRow(ReferenceValidator.Violation violation)
    {
        var serializedObject = new SerializedObject(violation.Component);
        var property = serializedObject.FindProperty(violation.PropertyPath);

        EditorGUILayout.BeginHorizontal();

        DrawStatusMarker(property);

        string objectName = violation.Component.gameObject.name;
        string fieldLabel = $"{violation.Component.GetType().Name}.{violation.PropertyPath}";

        if (GUILayout.Button(objectName, GUILayout.Width(160)))
        {
            Selection.activeGameObject = violation.Component.gameObject;
            EditorGUIUtility.PingObject(violation.Component.gameObject);
        }

        EditorGUILayout.LabelField(fieldLabel, GUILayout.MinWidth(180));

        if (property != null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.MinWidth(150));

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawStatusMarker(SerializedProperty property)
    {
        bool isResolved = property != null
                          && property.propertyType == SerializedPropertyType.ObjectReference
                          && property.objectReferenceValue != null;

        Color color = isResolved ? ResolvedColor : ProblemColor;

        var rect = GUILayoutUtility.GetRect(12f, 12f, GUILayout.Width(12f));
        rect.y += 3f;
        rect.height = 12f;
        EditorGUI.DrawRect(rect, color);
    }
}
