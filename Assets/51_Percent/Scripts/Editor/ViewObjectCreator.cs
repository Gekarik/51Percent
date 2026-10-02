using UnityEditor;
using UnityEngine;

public static class ViewObjectCreator
{
    [MenuItem("Tools/51 Percent/Create View Object")]
    private static void CreateViewObject()
    {
        var root = ObjectFactory.CreateGameObject("Model");
        GameObjectUtility.SetParentAndAlign(root, Selection.activeGameObject);

        var view = ObjectFactory.CreateGameObject("View", typeof(MeshFilter), typeof(MeshRenderer));
        view.transform.SetParent(root.transform, false);

        Selection.activeGameObject = root;
    }
}
