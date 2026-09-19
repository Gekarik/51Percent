using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Генерирует меш гекс-призмы из существующего меша гекса: контур берётся из вершин источника
// (след в плане XZ не меняется — спейсинг сетки сохраняется), верхняя грань остаётся на своём
// уровне, юбка вытягивается только вниз. Дна нет — его никто не видит.
// Повторный запуск перезаписывает содержимое ассета, сохраняя GUID и все ссылки на него.
public class HexPrismMeshCreator : ScriptableWizard
{
    private const string SavePath = "Assets/51_Percent/Meshes/Generated/HexPrism.asset";
    private const string MeshName = "HexPrism";
    private const string CreateButtonLabel = "Создать";
    private const string WindowTitle = "Hex Prism Mesh";

    [Header("Источник контура (текущий меш гекса)")]
    [SerializeField] private Mesh _sourceMesh;

    [Header("Глубина юбки вниз от верхней грани")]
    [SerializeField] private float _depth = 1f;

    [MenuItem("Tools/51 Percent/Create Hex Prism Mesh")]
    private static void Open()
    {
        DisplayWizard<HexPrismMeshCreator>(WindowTitle, CreateButtonLabel);
    }

    private void OnWizardUpdate()
    {
        if (_sourceMesh == null)
            errorString = "Укажи исходный меш гекса";
        else if (_depth <= 0f)
            errorString = "Глубина должна быть положительной";
        else
            errorString = string.Empty;

        isValid = errorString.Length == 0;
    }

    private void OnWizardCreate()
    {
        List<Vector2> outline = BuildConvexOutline(_sourceMesh.vertices);
        float topY = _sourceMesh.bounds.max.y;

        Mesh target = AssetDatabase.LoadAssetAtPath<Mesh>(SavePath);
        bool isNewAsset = target == null;

        if (isNewAsset)
            target = new Mesh();

        FillPrism(target, outline, topY, _depth);

        if (isNewAsset)
            AssetDatabase.CreateAsset(target, SavePath);
        else
            EditorUtility.SetDirty(target);

        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(target);
        Selection.activeObject = target;

        Debug.Log($"Меш призмы сохранён: {SavePath} (углов контура: {outline.Count}, глубина: {_depth})");
    }

    // Выпуклая оболочка вершин в плане XZ (monotone chain).
    // Дубликаты и коллинеарные точки отсеиваются условием <= 0 — на выходе чистые углы гекса
    private static List<Vector2> BuildConvexOutline(Vector3[] vertices)
    {
        var points = new List<Vector2>(vertices.Length);

        foreach (Vector3 vertex in vertices)
            points.Add(new Vector2(vertex.x, vertex.z));

        points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        var hull = new List<Vector2>();

        foreach (Vector2 point in points)
            AppendToChain(hull, point, 2);

        int lowerChainCount = hull.Count + 1;

        for (int i = points.Count - 2; i >= 0; i--)
            AppendToChain(hull, points[i], lowerChainCount);

        hull.RemoveAt(hull.Count - 1);

        EnsureClockwiseFromAbove(hull);
        return hull;
    }

    private static void AppendToChain(List<Vector2> chain, Vector2 point, int minCount)
    {
        while (chain.Count >= minCount && Cross(chain[^2], chain[^1], point) <= 0f)
            chain.RemoveAt(chain.Count - 1);

        chain.Add(point);
    }

    private static float Cross(Vector2 origin, Vector2 a, Vector2 b)
    {
        return (a.x - origin.x) * (b.y - origin.y) - (a.y - origin.y) * (b.x - origin.x);
    }

    // Юнити считает лицевой стороной обход по часовой стрелке со стороны нормали:
    // для верхней грани контур должен идти по часовой при взгляде сверху
    private static void EnsureClockwiseFromAbove(List<Vector2> outline)
    {
        float signedArea = 0f;

        for (int i = 0; i < outline.Count; i++)
        {
            Vector2 current = outline[i];
            Vector2 next = outline[(i + 1) % outline.Count];
            signedArea += current.x * next.y - next.x * current.y;
        }

        if (signedArea > 0f)
            outline.Reverse();
    }

    // Вершины не переиспользуются между гранями — жёсткие рёбра для плоского low-poly шейдинга
    private static void FillPrism(Mesh target, List<Vector2> outline, float topY, float depth)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        AppendTopCap(outline, topY, vertices, triangles, uvs);
        AppendSkirt(outline, topY, depth, vertices, triangles, uvs);

        target.Clear();
        target.name = MeshName;
        target.SetVertices(vertices);
        target.SetTriangles(triangles, 0);
        target.SetUVs(0, uvs);
        target.RecalculateNormals();
        target.RecalculateBounds();
    }

    private static void AppendTopCap(List<Vector2> outline, float topY,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vector2 centroid = Vector2.zero;

        foreach (Vector2 corner in outline)
            centroid += corner;

        centroid /= outline.Count;

        Bounds2D uvBounds = Bounds2D.FromPoints(outline);

        int centroidIndex = vertices.Count;
        vertices.Add(new Vector3(centroid.x, topY, centroid.y));
        uvs.Add(uvBounds.Normalize(centroid));

        int ringStart = vertices.Count;

        foreach (Vector2 corner in outline)
        {
            vertices.Add(new Vector3(corner.x, topY, corner.y));
            uvs.Add(uvBounds.Normalize(corner));
        }

        for (int i = 0; i < outline.Count; i++)
        {
            triangles.Add(centroidIndex);
            triangles.Add(ringStart + i);
            triangles.Add(ringStart + (i + 1) % outline.Count);
        }
    }

    private static void AppendSkirt(List<Vector2> outline, float topY, float depth,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        float bottomY = topY - depth;

        for (int i = 0; i < outline.Count; i++)
        {
            Vector2 a = outline[i];
            Vector2 b = outline[(i + 1) % outline.Count];

            int baseIndex = vertices.Count;
            vertices.Add(new Vector3(a.x, topY, a.y));
            vertices.Add(new Vector3(b.x, topY, b.y));
            vertices.Add(new Vector3(a.x, bottomY, a.y));
            vertices.Add(new Vector3(b.x, bottomY, b.y));

            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));

            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);

            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }
    }

    // Прямоугольник контура в плане — для нормировки UV верхней грани
    private readonly struct Bounds2D
    {
        private readonly Vector2 _min;
        private readonly Vector2 _size;

        private Bounds2D(Vector2 min, Vector2 size)
        {
            _min = min;
            _size = size;
        }

        public static Bounds2D FromPoints(List<Vector2> points)
        {
            Vector2 min = points[0];
            Vector2 max = points[0];

            foreach (Vector2 point in points)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return new Bounds2D(min, max - min);
        }

        public Vector2 Normalize(Vector2 point)
        {
            return new Vector2(
                _size.x > 0f ? (point.x - _min.x) / _size.x : 0f,
                _size.y > 0f ? (point.y - _min.y) / _size.y : 0f);
        }
    }
}
