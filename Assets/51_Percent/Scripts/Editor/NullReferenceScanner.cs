using UnityEditor;
using UnityEngine;

public static class NullReferenceScanner
{
    [MenuItem("EditorFeatures/Scan Null References in Scene")]
    private static void ScanScene()
    {
        var components = Object.FindObjectsOfType<MonoBehaviour>(true);
        int nullCount = 0;
        int missingCount = 0;

        foreach (var component in components)
        {
            var serializedObject = new SerializedObject(component);
            var property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                bool isMissing = property.objectReferenceValue == null
                                 && property.objectReferenceInstanceIDValue != 0;
                bool isNull = property.objectReferenceValue == null
                              && property.objectReferenceInstanceIDValue == 0;

                if (isMissing)
                {
                    Debug.LogError(
                        $"Сломанная ссылка: <b>{component.gameObject.name}</b> → {component.GetType().Name}.{property.name}",
                        component.gameObject);
                    missingCount++;
                }
                else if (isNull)
                {
                    Debug.LogWarning(
                        $"Null ссылка: <b>{component.gameObject.name}</b> → {component.GetType().Name}.{property.name}",
                        component.gameObject);
                    nullCount++;
                }
            }
        }

        Debug.Log($"Сканирование завершено. Сломанных: {missingCount}, Null: {nullCount}.");
    }
}
