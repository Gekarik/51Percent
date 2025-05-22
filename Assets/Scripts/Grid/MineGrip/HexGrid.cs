using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private HexGridData gridData;

    private Dictionary<AxialCoord, Hex> hexByCoord;
    private Dictionary<Hex, IReadOnlyList<Hex>> neighborsCache;

    private void Awake()
    {
        BuildRuntimeDictionary();
    }

    private void BuildRuntimeDictionary()
    {
        Debug.Log("[HexGrid] BuildRuntimeDictionary started.");

        Hex[] children = GetComponentsInChildren<Hex>();
        Debug.Log($"[HexGrid] Found {children.Length} Hex children.");

        var lookup = new Dictionary<AxialCoord, Hex>(children.Length);
        foreach (var hex in children)
        {
            if (hex == null)
            {
                Debug.LogWarning("[HexGrid] Found null Hex in children.");
                continue;
            }

            lookup[hex.Coord] = hex;
            Debug.Log($"[HexGrid] Added Hex to lookup with Coord: {hex.Coord}");
        }

        int expectedCount = gridData.Coords.Count;
        Debug.Log($"[HexGrid] gridData has {expectedCount} coords.");

        hexByCoord = new Dictionary<AxialCoord, Hex>(expectedCount);

        foreach (var coord in gridData.Coords)
        {
            if (lookup.TryGetValue(coord, out var hex))
                hexByCoord[coord] = hex;
            else
                Debug.LogWarning($"[HexGrid] No Hex found for Coord: {coord}");
        }

        neighborsCache = new Dictionary<Hex, IReadOnlyList<Hex>>();
    }

    public IReadOnlyList<Hex> AllHexes => hexByCoord.Values.ToList();

    public Hex GetRandomHex()
    {
        var allHexes = AllHexes;
        if (allHexes.Count == 0) return null;

        int index = UnityEngine.Random.Range(0, allHexes.Count);
        return allHexes[index];
    }

    private IReadOnlyList<Hex> GetNeighbors(Hex tile, float searchRadius = 1.5f)
    {
        var neighbors = new List<Hex>();
        var colliders = Physics.OverlapSphere(tile.transform.position, searchRadius);

        foreach (var collider in colliders)
        {
            Hex neighbor = collider.GetComponent<Hex>();
            if (neighbor != null && neighbor != tile)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }


    public IReadOnlyList<Hex> GetNeighborsCached(Hex tile)
    {
        if (neighborsCache.TryGetValue(tile, out var cachedNeighbors))
            return cachedNeighbors;

        var neighbors = GetNeighbors(tile);
        neighborsCache[tile] = neighbors;
        return neighbors;
    }
}
