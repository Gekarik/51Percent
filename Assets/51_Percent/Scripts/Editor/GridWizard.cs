using UnityEditor;
using UnityEngine;

public class GridWizard : EditorWindow
{
    private const string PrefabGuidKey = "51Percent.GridWizard.HexPrefabGuid";
    private const string RadiusKey = "51Percent.GridWizard.HexRadius";
    private const string VisualScaleKey = "51Percent.GridWizard.VisualScale";
    private const string BuildWallsKey = "51Percent.GridWizard.BuildWalls";
    private const string WallHeightKey = "51Percent.GridWizard.WallHeight";
    private const string WallThicknessKey = "51Percent.GridWizard.WallThickness";

    private const float DefaultRadius = 1f;
    private const float DefaultVisualScale = 0.9f;
    private const float MinVisualScale = 0.5f;
    private const float MaxVisualScale = 1f;
    private const float DefaultWallHeight = 5f;
    private const float DefaultWallThickness = 1f;
    private const string WallsRootName = "Walls";

    private const float RowHeightMultiplier = 0.75f;
    private const float TrimTolerance = 1e-4f;
    private const string UndoOperationName = "Generate Hex Grid";

    private Hex _hexPrefab;
    private float _hexRadius;
    private float _visualScale;
    private BoxCollider _playableArea;
    private bool _buildWalls;
    private float _wallHeight;
    private float _wallThickness;

    [MenuItem("Tools/51 Percent/Grid Wizard")]
    private static void Open()
    {
        GetWindow<GridWizard>("Grid Wizard");
    }

    private void OnEnable()
    {
        LoadPreferences();

        if (_playableArea == null)
            _playableArea = FindPlayableArea();
    }

    private void OnDisable()
    {
        SavePreferences();
    }

    private void OnGUI()
    {
        _hexPrefab = (Hex)EditorGUILayout.ObjectField("Префаб гекса", _hexPrefab, typeof(Hex), false);
        _hexRadius = EditorGUILayout.FloatField("Радиус гекса", _hexRadius);
        _visualScale = EditorGUILayout.Slider("Масштаб вьюшки", _visualScale, MinVisualScale, MaxVisualScale);
        _playableArea = (BoxCollider)EditorGUILayout.ObjectField("Игровая зона", _playableArea, typeof(BoxCollider), true);

        _buildWalls = EditorGUILayout.Toggle("Строить стены", _buildWalls);

        using (new EditorGUI.DisabledScope(!_buildWalls))
        {
            _wallHeight = EditorGUILayout.FloatField("Высота стен", _wallHeight);
            _wallThickness = EditorGUILayout.FloatField("Толщина стен", _wallThickness);
        }

        var grid = FindObjectOfType<HexGrid>();

        if (grid == null)
        {
            EditorGUILayout.HelpBox("В сцене нет HexGrid — некуда класть гексы.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Контейнер", grid.name);

        using (new EditorGUI.DisabledScope(!CanGenerate()))
        {
            if (GUILayout.Button("Перегенерировать"))
                Regenerate(grid.transform);
        }

        if (GUILayout.Button("Очистить"))
            Clear(grid.transform);
    }

    private bool CanGenerate()
    {
        return _hexPrefab != null && _playableArea != null && _hexRadius > 0f;
    }

    private void Regenerate(Transform container)
    {
        Undo.SetCurrentGroupName(UndoOperationName);
        Clear(container);
        Generate(container);

        if (_buildWalls)
            RebuildWalls();
    }

    private void Generate(Transform container)
    {
        Bounds bounds = _playableArea.bounds;

        var meshFilter = _hexPrefab.HexView.GetComponent<MeshFilter>();
        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        float hexWidth = meshSize.x * _hexRadius;
        float hexHeight = meshSize.z * _hexRadius;

        int cols = Mathf.FloorToInt(bounds.size.x / hexWidth);
        int rows = Mathf.FloorToInt(bounds.size.z / (hexHeight * RowHeightMultiplier));

        float halfWidth = hexWidth * 0.5f;
        float halfHeight = hexHeight * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < cols; q++)
            {
                float x = bounds.min.x
                        + q * hexWidth
                        + ((r & 1) == 1 ? hexWidth * 0.5f : 0f)
                        + hexWidth * 0.5f;
                float z = bounds.min.z
                        + r * hexHeight * RowHeightMultiplier
                        + hexHeight * 0.5f;

                if (x - halfWidth < bounds.min.x - TrimTolerance || x + halfWidth > bounds.max.x + TrimTolerance)
                    continue;

                if (z - halfHeight < bounds.min.z - TrimTolerance || z + halfHeight > bounds.max.z + TrimTolerance)
                    continue;

                CreateHex(container, new Vector3(x, 0f, z), new HexCoord(q, r));
            }
        }
    }

    private void CreateHex(Transform container, Vector3 position, HexCoord coord)
    {
        var hexInstance = (Hex)PrefabUtility.InstantiatePrefab(_hexPrefab, container);
        hexInstance.transform.SetPositionAndRotation(position, Quaternion.identity);
        hexInstance.transform.localScale = Vector3.one * _hexRadius;
        hexInstance.HexView.transform.localScale = Vector3.one * _visualScale;
        hexInstance.AssignCoord(coord);
        Undo.RegisterCreatedObjectUndo(hexInstance.gameObject, UndoOperationName);
    }

    private static void Clear(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
    }

    private void RebuildWalls()
    {
        Transform root = GetWallsRoot();
        Clear(root);

        Bounds bounds = _playableArea.bounds;
        float halfThickness = _wallThickness * 0.5f;

        float horizontalLength = bounds.size.x + _wallThickness * 2f;

        CreateWall(root, "Wall_North",
            new Vector3(bounds.center.x, bounds.center.y, bounds.max.z + halfThickness),
            new Vector3(horizontalLength, _wallHeight, _wallThickness));

        CreateWall(root, "Wall_South",
            new Vector3(bounds.center.x, bounds.center.y, bounds.min.z - halfThickness),
            new Vector3(horizontalLength, _wallHeight, _wallThickness));

        CreateWall(root, "Wall_East",
            new Vector3(bounds.max.x + halfThickness, bounds.center.y, bounds.center.z),
            new Vector3(_wallThickness, _wallHeight, bounds.size.z));

        CreateWall(root, "Wall_West",
            new Vector3(bounds.min.x - halfThickness, bounds.center.y, bounds.center.z),
            new Vector3(_wallThickness, _wallHeight, bounds.size.z));
    }

    private static Transform GetWallsRoot()
    {
        var existing = GameObject.Find(WallsRootName);

        if (existing != null)
            return existing.transform;

        var root = new GameObject(WallsRootName);
        Undo.RegisterCreatedObjectUndo(root, UndoOperationName);
        return root.transform;
    }

    private static void CreateWall(Transform parent, string wallName, Vector3 position, Vector3 size)
    {
        var wall = new GameObject(wallName);
        wall.transform.SetParent(parent, worldPositionStays: false);
        wall.transform.position = position;
        wall.AddComponent<BoxCollider>().size = size;
        Undo.RegisterCreatedObjectUndo(wall, UndoOperationName);
    }

    private static BoxCollider FindPlayableArea()
    {
        var spawner = FindObjectOfType<CollectibleSpawnerBase>();

        if (spawner == null)
            return null;

        var serializedObject = new SerializedObject(spawner);
        var spawnPoint = serializedObject.FindProperty("_spawnPoint");

        return spawnPoint?.FindPropertyRelative("_area")?.objectReferenceValue as BoxCollider;
    }

    private void LoadPreferences()
    {
        _hexRadius = EditorPrefs.GetFloat(RadiusKey, DefaultRadius);
        _visualScale = EditorPrefs.GetFloat(VisualScaleKey, DefaultVisualScale);
        _buildWalls = EditorPrefs.GetBool(BuildWallsKey, true);
        _wallHeight = EditorPrefs.GetFloat(WallHeightKey, DefaultWallHeight);
        _wallThickness = EditorPrefs.GetFloat(WallThicknessKey, DefaultWallThickness);

        string guid = EditorPrefs.GetString(PrefabGuidKey, string.Empty);

        if (guid.Length > 0)
            _hexPrefab = AssetDatabase.LoadAssetAtPath<Hex>(AssetDatabase.GUIDToAssetPath(guid));
    }

    private void SavePreferences()
    {
        EditorPrefs.SetFloat(RadiusKey, _hexRadius);
        EditorPrefs.SetFloat(VisualScaleKey, _visualScale);
        EditorPrefs.SetBool(BuildWallsKey, _buildWalls);
        EditorPrefs.SetFloat(WallHeightKey, _wallHeight);
        EditorPrefs.SetFloat(WallThicknessKey, _wallThickness);

        if (_hexPrefab != null
            && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_hexPrefab, out string guid, out long _))
            EditorPrefs.SetString(PrefabGuidKey, guid);
    }
}
