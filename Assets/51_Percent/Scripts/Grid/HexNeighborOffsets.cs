using UnityEngine;

[CreateAssetMenu(fileName = "HexNeighborOffsets", menuName = "Hex/NeighborOffsets")]
public class HexNeighborOffsets : ScriptableObject
{
    private const int DirectionsCount = 6;

    [Header("Offsets for even rows (0, 2, 4...)")]
    [SerializeField] private Vector2Int[] _evenRowOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    [Header("Offsets for odd rows (1, 3, 5...)")]
    [SerializeField] private Vector2Int[] _oddRowOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    public int Count => DirectionsCount;

    public HexCoord GetOffset(int index, bool isOddRow)
    {
        Vector2Int offset = isOddRow ? _oddRowOffsets[index] : _evenRowOffsets[index];
        return new HexCoord(offset.x, offset.y);
    }
}
