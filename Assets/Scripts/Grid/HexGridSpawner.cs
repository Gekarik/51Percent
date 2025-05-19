using UnityEngine;

public class HexGridSpawner : MonoBehaviour
{
    [SerializeField] private Hex _prefab;
    [SerializeField] private BoxCollider _playableArea;
    [SerializeField] private float _hexRadius;
    [SerializeField] private Transform _container;
    [SerializeField] private HexGridData _hexGridData;
    [SerializeField] private HexGrid _hexGrid;

    private void Start()
    {
        _hexGrid.BuildRuntimeDictionary(_container, _hexGridData);
    }

    [ContextMenu("RegenerateGrid")]
    private void RegenerateGrid()
    {
        ClearGrid();
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        ClearGrid();

        _prefab.transform.localScale = Vector3.one * _hexRadius;
        Vector3 size = _prefab.GetComponent<Renderer>().bounds.size;
        float w = size.x;
        float h = size.z;

        var bounds = _playableArea.bounds;
        int cols = Mathf.FloorToInt(bounds.size.x / w);
        int rows = Mathf.FloorToInt(bounds.size.z / (h * 0.75f));

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < cols; q++)
            {
                Vector3 pos = bounds.min + new Vector3(
                    q * w + (r % 2 == 1 ? w * 0.5f : 0) + w / 2,
                    0,
                    r * h * 0.75f + h / 2
                );

                var hex = Instantiate(_prefab, pos, Quaternion.identity, _container);
                var coord = new AxialCoord(q, r);
                hex.SetCoord(coord);

                _hexGridData.RegisterCoord(coord);
            }
        }

        _hexGrid.BuildRuntimeDictionary(_container, _hexGridData);
    }

    private void ClearGrid()
    {
        _hexGridData.Clear();
        for (int i = _container.childCount - 1; i >= 0; i--)
            DestroyImmediate(_container.GetChild(i).gameObject);
    }
}
