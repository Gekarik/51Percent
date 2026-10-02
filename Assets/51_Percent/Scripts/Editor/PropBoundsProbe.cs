using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PropBoundsProbe
{
    private static readonly string[] Props =
    {
        "Assets/51_Percent/Prefabs/Props/Worn/Skateboard_Worn.prefab",
        "Assets/51_Percent/Prefabs/Props/Worn/Wings_Worn.prefab",
    };

    [MenuItem("Tools/51 Percent/Probe Prop Bounds")]
    private static void Probe()
    {
        foreach (string path in Props)
            Debug.Log(Describe(path));
    }

    private const float MiddleFraction = 0.33f;

    private static string DescribeStandingSurface(GameObject instance)
    {
        var report = new StringBuilder();

        foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = filter.sharedMesh;

            if (mesh == null)
                continue;

            var vertices = mesh.vertices;
            float halfLength = mesh.bounds.size.z * MiddleFraction * 0.5f;
            float centerZ = mesh.bounds.center.z;
            float top = float.MinValue;
            int counted = 0;

            foreach (var vertex in vertices)
            {
                if (Mathf.Abs(vertex.z - centerZ) > halfLength)
                    continue;

                top = Mathf.Max(top, filter.transform.TransformPoint(vertex).y);
                counted++;
            }

            if (counted > 0)
                report.Append($" ||| {filter.name}: настил в середине {top:F4} (вершин {counted})");
        }

        return report.ToString();
    }

    private static string Describe(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
            return $"PROP_BOUNDS: {path} не найден";

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        try
        {
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
                return $"PROP_BOUNDS: {prefab.name} — нет рендереров";

            var bounds = renderers[0].bounds;

            foreach (var renderer in renderers.Skip(1))
                bounds.Encapsulate(renderer.bounds);

            var report = new StringBuilder();
            report.Append($"PROP_BOUNDS: {prefab.name}");
            report.Append($" | масштаб корня {instance.transform.localScale.x:F3}");
            report.Append($" | размер {bounds.size.x:F3} x {bounds.size.y:F3} x {bounds.size.z:F3}");
            report.Append($" | низ {bounds.min.y:F4}, верх {bounds.max.y:F4}");
            report.Append($" | центр Y {bounds.center.y:F4}");

            foreach (var renderer in renderers)
                report.Append($" || {renderer.name}: низ {renderer.bounds.min.y:F4}, верх {renderer.bounds.max.y:F4}");

            report.Append(DescribeStandingSurface(instance));
            return report.ToString();
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }
}
