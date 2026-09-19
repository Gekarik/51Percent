using System.Linq;
using UnityEditor;
using UnityEngine;

// Собирает надеваемые пропы из тех же моделей, что и пикапы. Пикап и надетый проп —
// соседи, а не родственники: у пикапа есть поведение коллектибла (покачивание,
// вращение, коллайдер), надетому пропу всё это только мешает. Вариант префаба тут
// не подходит — пришлось бы наследовать поведение, чтобы сразу его вычитать
public static class WornPropBuilder
{
    private const string OutputFolder = "Assets/51_Percent/Prefabs/Props/Worn";

    private readonly struct WornProp
    {
        public string SourcePath { get; }
        public string Name { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalEuler { get; }
        public float Scale { get; }

        public WornProp(string sourcePath, string name, Vector3 localPosition, Vector3 localEuler, float scale)
        {
            SourcePath = sourcePath;
            Name = name;
            LocalPosition = localPosition;
            LocalEuler = localEuler;
            Scale = scale;
        }
    }

    // Подгонка живёт в одном месте: в корневом трансформе надетого префаба.
    // Доска опущена на высоту настила деки: пивот у доски внизу, у колёс, поэтому
    // без сдвига настил оказывается выше ступней и персонаж проваливается внутрь.
    // Величина измерена по вершинам деки в её средней трети (Probe Prop Bounds)
    private const float SkateboardDeckHeight = 0.168f;

    // Доска сдвинута под опорную (левую) ногу. Толчковая правая уходит вбок на 0.10…0.26,
    // а полуширина доски 0.16 — стоя по осевой, она подставлялась бы под толчок.
    // Замерено Probe Foot Track по клипу катания
    private const float SkateboardSideShift = 0.08f;

    // Сокет спины сидит на кости spine_03, а её оси развёрнуты: X смотрит вперёд,
    // Z — влево (замерено Probe Socket Orientation). Поэтому размах крыльев,
    // лежащий по их собственной X, без поворота раскрывался бы вперёд-назад,
    // а отступ за спину задаётся по X сокета, а не по Z
    private const float WingsYaw = 90f;
    private const float WingsBackShift = 0.05f;

    private static readonly WornProp[] Props =
    {
        new WornProp("Assets/51_Percent/Prefabs/Props/Skateboard.prefab", "Skateboard_Worn",
            new Vector3(-SkateboardSideShift, -SkateboardDeckHeight, 0f), Vector3.zero, 1.3f),
        new WornProp("Assets/51_Percent/Prefabs/Props/Goodbyewing.prefab", "Wings_Worn",
            new Vector3(-WingsBackShift, 0f, 0f), new Vector3(0f, WingsYaw, 0f), 0.25f),
    };

    [MenuItem("Tools/51 Percent/Build Worn Props")]
    private static void Build()
    {
        EnsureFolder();

        foreach (var prop in Props)
            BuildOne(prop);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/51_Percent/Prefabs/Props", "Worn");
    }

    private static void BuildOne(WornProp prop)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(prop.SourcePath);

        if (source == null)
        {
            Debug.LogError($"Не найден исходный префаб {prop.SourcePath}.");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);

        try
        {
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = prop.Name;
            StripPickupBehaviour(instance);

            instance.transform.localPosition = prop.LocalPosition;
            instance.transform.localEulerAngles = prop.LocalEuler;
            instance.transform.localScale = Vector3.one * prop.Scale;

            string path = $"{OutputFolder}/{prop.Name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Debug.Log($"Надеваемый проп собран: {path}");
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    // Надетый проп — только картинка: ни покачивания, ни вращения, ни физики
    private static void StripPickupBehaviour(GameObject root)
    {
        foreach (var view in root.GetComponentsInChildren<CollectibleViewBase>(true))
            Object.DestroyImmediate(view, true);

        foreach (var body in root.GetComponentsInChildren<Rigidbody>(true))
            Object.DestroyImmediate(body, true);

        foreach (var collider in root.GetComponentsInChildren<Collider>(true).ToArray())
            Object.DestroyImmediate(collider, true);
    }
}
