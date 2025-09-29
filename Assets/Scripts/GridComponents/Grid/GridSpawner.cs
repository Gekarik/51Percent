using UnityEngine;

public class GridSpawner : MonoBehaviour, IMapBounds
{
    [SerializeField] private Hex _hexPrefab;
    [SerializeField] private float _hexRadius;
    [SerializeField] private BoxCollider _playableArea;
    [SerializeField] private Transform _container;

    private Bounds _bounds;

    [ContextMenu("Generate")]
    private void Generate()
    {
        Clear();
        _bounds = _playableArea.bounds;

        var meshFilter = _hexPrefab.HexView.GetComponent<MeshFilter>();
        var meshSize = meshFilter.sharedMesh.bounds.size;

        float hexWidth = meshSize.x * _hexRadius;
        float hexHeight = meshSize.z * _hexRadius;

        int cols = Mathf.FloorToInt(_bounds.size.x / hexWidth);
        int rows = Mathf.FloorToInt(_bounds.size.z / (hexHeight * 0.75f));

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < cols; q++)
            {
                float x = _bounds.min.x
                        + q * hexWidth
                        + ((r & 1) == 1 ? hexWidth * 0.5f : 0f)
                        + hexWidth * 0.5f;
                float z = _bounds.min.z
                        + r * hexHeight * 0.75f
                        + hexHeight * 0.5f;

                Vector3 pos = new Vector3(x, 0f, z);
                var hexInstance = Instantiate(_hexPrefab, pos, Quaternion.identity, _container);
                hexInstance.transform.localScale = Vector3.one * _hexRadius;
            }
        }
    }

    [ContextMenu("Regenerate")]
    private void Regenerate()
    {
        Clear();
        Generate();
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        for (int i = _container.childCount - 1; i >= 0; i--)
            DestroyImmediate(_container.GetChild(i).gameObject);
    }
    
    // IMapBounds implementation
    public Bounds PlayableArea 
    { 
        get 
        { 
            if (_bounds.size == Vector3.zero)
                _bounds = _playableArea.bounds;
            return _bounds; 
        } 
    }
    
    public bool IsInBounds(Vector3 position)
    {
        return PlayableArea.Contains(position);
    }
    
    public Vector3 ClampToBounds(Vector3 position)
    {
        var bounds = PlayableArea;
        return new Vector3(
            Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
            position.y,
            Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
        );
    }
}
