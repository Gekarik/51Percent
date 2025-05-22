using UnityEngine;

[ExecuteInEditMode]
public class HexGridSpawner : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [SerializeField] private HexTile _prefab;
    [SerializeField] private Transform _container;

    [Header("Grid Settings")]
    [Tooltip("Умножающий коэффициент масштаба префаба")]
    [SerializeField] private float _hexScale = 1f;
    [Tooltip("Область, в пределах которой нужно разместить тайлы")]
    [SerializeField] private BoxCollider _playableArea;

    [Header("Grid Data & Logic")]
    [SerializeField] private HexGridDataLegacy _hexGridData;
    [SerializeField] private HexGridLegacy _hexGrid;

    private void Start()
    {
        // если тайлы уже сгенерированы в редакторе, просто строим словарь
        _hexGrid.BuildRuntimeDictionary(_container, _hexGridData);
    }

    [ContextMenu("Regenerate Grid")]
    private void RegenerateGrid()
    {
        ClearGrid();
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        ClearGrid();

        if (_prefab == null || _playableArea == null || _hexGridData == null || _hexGrid == null)
        {
            Debug.LogError("HexGridSpawner: не все ссылки настроены!", this);
            return;
        }

        // 1) выставляем масштаб префаба
        _prefab.transform.localScale = Vector3.one * _hexScale;

        // 2) берём вложенный Renderer, чтобы вычислить world-size
        var rend = _prefab.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("HexGridSpawner: у префаба нет Renderer ни в корне, ни в детях!", this);
            return;
        }
        Vector3 size = rend.bounds.size;
        float w = size.x;
        float h = size.z;

        // 3) считаем количество строк и колонок
        var bounds = _playableArea.bounds;
        int cols = Mathf.FloorToInt(bounds.size.x / w);
        int rows = Mathf.FloorToInt(bounds.size.z / (h * 0.75f));

        // 4) генерация тайлов (flat-top example)
        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < cols; q++)
            {
                // смещение столбцов: каждый нечётный сдвигаем по X на пол-ширины
                float x = bounds.min.x + q * w + ((r % 2) == 1 ? w * 0.5f : 0f) + w * 0.5f;
                float z = bounds.min.z + r * h * 0.75f + h * 0.5f;

                Vector3 pos = new Vector3(x, 0f, z);
                var hex = Instantiate(_prefab, pos, Quaternion.identity, _container);

                // привязываем координаты и регистрируем
                var coord = new AxialCoord(q, r);
                hex.SetCoord(coord);

                _hexGridData.RegisterCoord(coord);
            }
        }

        // 5) обновляем runtime-словарь
        _hexGrid.BuildRuntimeDictionary(_container, _hexGridData);
    }

    private void ClearGrid()
    {
        if (_hexGridData != null)
            _hexGridData.Clear();

        if (_container == null) return;
        for (int i = _container.childCount - 1; i >= 0; i--)
            DestroyImmediate(_container.GetChild(i).gameObject);
    }
}
