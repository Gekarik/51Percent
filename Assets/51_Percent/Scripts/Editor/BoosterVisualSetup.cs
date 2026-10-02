using System;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public static class BoosterVisualSetup
{
    private const string RegistryPath = "Assets/51_Percent/ScriptableObjects/BoosterVisuals.asset";
    private const string SkateboardPath = "Assets/51_Percent/Prefabs/Props/Worn/Skateboard_Worn.prefab";
    private const string WingsPath = "Assets/51_Percent/Prefabs/Props/Worn/Wings_Worn.prefab";
    private const string BackSocketName = "BackSocket";
    private const string FeetSocketName = "FeetSocket";
    private const string SpineBoneName = "spine_03";

    [MenuItem("Tools/51 Percent/Configure Booster Visuals")]
    private static void Configure()
    {
        var registry = LoadOrCreateRegistry();
        FillDefaults(registry);
        ConfigurePrefabs(registry);
        ValidateProps();
        Debug.Log("Booster visuals: реестр заведён, компоненты и сокеты прошиты на Player и Enemy.");
    }

    private static void ValidateProps()
    {
        foreach (string path in new[] { SkateboardPath, WingsPath })
        {
            var prop = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prop == null)
            {
                Debug.LogError($"Надеваемый проп {path} не найден. Сначала выполни Tools/51 Percent/Build Worn Props.");
                continue;
            }

            if (prop.GetComponentInChildren<CollectibleViewBase>(true) != null)
                Debug.LogError($"На {path} висит поведение коллектибла: на персонаже проп будет крутиться и качаться.");

            if (prop.GetComponentInChildren<Collider>(true) != null)
                Debug.LogError($"На {path} остался коллайдер: надетый проп не должен участвовать в физике.");
        }
    }

    private static BoosterVisualRegistry LoadOrCreateRegistry()
    {
        var existing = AssetDatabase.LoadAssetAtPath<BoosterVisualRegistry>(RegistryPath);

        if (existing != null)
            return existing;

        var created = ScriptableObject.CreateInstance<BoosterVisualRegistry>();
        AssetDatabase.CreateAsset(created, RegistryPath);
        return created;
    }

    private static void FillDefaults(BoosterVisualRegistry registry)
    {
        var serialized = new SerializedObject(registry);
        var entries = serialized.FindProperty("_entries");
        entries.ClearArray();

        AddEntry(entries, BoosterId.Speed, LocomotionMode.Skating,
            AssetDatabase.LoadAssetAtPath<GameObject>(SkateboardPath), SocketType.Feet, 0f, Ease.Linear,
            0f, 0f, 0f, Ease.OutSine);

        AddEntry(entries, BoosterId.Mushroom, LocomotionMode.Default,
            null, SocketType.Back, 0f, Ease.Linear,
            1.5f, 1f, 0.35f, Ease.OutBack);

        AddEntry(entries, BoosterId.Wings, LocomotionMode.Default,
            AssetDatabase.LoadAssetAtPath<GameObject>(WingsPath), SocketType.Back, 0.35f, Ease.InBack,
            0f, 0f, 0f, Ease.OutSine);

        AddEntry(entries, BoosterId.Spikes, LocomotionMode.Default,
            null, SocketType.Back, 0f, Ease.Linear,
            0f, 0f, 0f, Ease.OutSine);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }

    private static void AddEntry(SerializedProperty entries, BoosterId id, LocomotionMode mode,
        GameObject prop, SocketType socket, float outroDuration, Ease outroEase,
        float modelScale, float growDuration, float shrinkDuration, Ease scaleEase)
    {
        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);
        var entry = entries.GetArrayElementAtIndex(index);

        SetEnum(entry.FindPropertyRelative("_id"), id);
        SetEnum(entry.FindPropertyRelative("_mode"), mode);
        entry.FindPropertyRelative("_propPrefab").objectReferenceValue = prop;
        SetEnum(entry.FindPropertyRelative("_socket"), socket);
        entry.FindPropertyRelative("_propOutroDuration").floatValue = outroDuration;
        SetEnum(entry.FindPropertyRelative("_propOutroEase"), outroEase);
        entry.FindPropertyRelative("_modelScale").floatValue = modelScale;
        entry.FindPropertyRelative("_growDuration").floatValue = growDuration;
        entry.FindPropertyRelative("_shrinkDuration").floatValue = shrinkDuration;
        SetEnum(entry.FindPropertyRelative("_scaleEase"), scaleEase);
    }

    private static void SetEnum<T>(SerializedProperty property, T value) where T : Enum
    {
        property.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(T)), value);
    }

    private static void ConfigurePrefabs(BoosterVisualRegistry registry)
    {
        foreach (string name in new[] { "Player", "Enemy" })
        {
            string path = $"Assets/51_Percent/Prefabs/Characters/{name}.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                var model = root.GetComponentInChildren<CharacterView>().transform;
                var back = EnsureSocket(FindBone(model, SpineBoneName) ?? model, BackSocketName);
                var feet = EnsureSocket(model, FeetSocketName);

                AssignRegistry(Require<BoosterPresentation>(root), registry);
                AssignSockets(root.GetComponent<CharacterBase>(), back, feet);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }

    private static Transform EnsureSocket(Transform parent, string name)
    {
        var existing = parent.Find(name);

        if (existing != null)
            return existing;

        var created = new GameObject(name).transform;
        created.SetParent(parent, false);
        created.localPosition = Vector3.zero;
        created.localRotation = Quaternion.identity;
        return created;
    }

    private static Transform FindBone(Transform model, string name)
    {
        return model.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(child => child.name == name);
    }

    private static T Require<T>(GameObject root) where T : Component
    {
        return root.GetComponent<T>() ?? root.AddComponent<T>();
    }

    private static void AssignRegistry(Component target, BoosterVisualRegistry registry)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty("_registry").objectReferenceValue = registry;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignSockets(CharacterBase character, Transform back, Transform feet)
    {
        var serialized = new SerializedObject(character);
        serialized.FindProperty("_backSocket").objectReferenceValue = back;
        serialized.FindProperty("_feetSocket").objectReferenceValue = feet;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
