using UnityEditor;
using UnityEngine;

public static class EditorFeaturesMenu
{
    [MenuItem("EditorFeatures/Create View Object")]
    private static void CreateViewObject()
    {
        var parent = new GameObject("NewObject");
        Undo.RegisterCreatedObjectUndo(parent, "Create View Object");

        var view = new GameObject("View");
        Undo.RegisterCreatedObjectUndo(view, "Create View Object");
        view.transform.SetParent(parent.transform, false);

        Selection.activeGameObject = parent;
    }
}
