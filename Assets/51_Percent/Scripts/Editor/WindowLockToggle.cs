using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class WindowLockToggle
{
    [MenuItem("Window/Utils/Toggle Lock %t")]
    private static void ToggleLock()
    {
        var focused = EditorWindow.focusedWindow;

        if (GetLockProperty(focused) != null)
        {
            ToggleWindowLock(focused);
            return;
        }

        if (Selection.activeObject != null)
            ToggleWindowLock(GetInspectorWindow());
    }

    private static EditorWindow GetInspectorWindow()
    {
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        var focused = EditorWindow.focusedWindow;
        if (focused != null && focused.GetType() == inspectorType)
            return focused;

        var inspectors = Resources.FindObjectsOfTypeAll(inspectorType);
        return inspectors.Length > 0 ? inspectors[0] as EditorWindow : null;
    }

    private static void ToggleWindowLock(EditorWindow window)
    {
        var prop = GetLockProperty(window);
        if (prop == null)
            return;

        prop.SetValue(window, !(bool)prop.GetValue(window));
        window.Repaint();
    }

    private static PropertyInfo GetLockProperty(EditorWindow window)
    {
        if (window == null)
            return null;

        var prop = window.GetType().GetProperty("isLocked",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        return prop != null && prop.CanWrite ? prop : null;
    }
}
