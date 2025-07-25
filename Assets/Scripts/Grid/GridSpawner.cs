using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [SerializeField] private Hex _hexPrefab;
    [SerializeField] private float _hexRadius;
    [SerializeField] private BoxCollider _playableArea;
    [SerializeField] private Transform _container;

    private Bounds _bounds;

    [ContextMenu("Regenerate")]
    private void RegenerateGrid()
    {
        Clear();
        Generate();
    }

    [ContextMenu("Generate")]
    private void Generate()
    {
        _bounds = _playableArea.bounds;
        _hexPrefab.transform.localScale = Vector3.one * _hexRadius;
        var hexSize = _hexPrefab.GetRendererBounds().size;

        var areaWidth = _bounds.size.x;
        var areaHeight = _bounds.size.z;

        float hexWidth = hexSize.x;
        float hexHight = hexSize.z;

        int cols = Mathf.FloorToInt(_bounds.size.x / hexWidth);
        int rows = Mathf.FloorToInt(_bounds.size.z / (hexHight * 0.75f));

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < cols; q++)
            {
                float x = _bounds.min.x + q * hexWidth + ((r % 2) == 1 ? hexWidth * 0.5f : 0f) + hexWidth * 0.5f;
                float z = _bounds.min.z + r * hexHight * 0.75f + hexHight * 0.5f;

                Vector3 pos = new Vector3(x, 0f, z);
                var hex = Instantiate(_hexPrefab, pos, Quaternion.identity, _container);
            }
        }
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        while (_container.childCount > 0)
            DestroyImmediate(_container.GetChild(0).gameObject);
    }
}
