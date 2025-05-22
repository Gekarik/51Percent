using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private HexGridData _hexGridData;
    private Transform _hexContainer;

    private readonly Dictionary<AxialCoord, Hex> _hexByCoord = new();

    private void Awake()
    {
        _hexContainer = transform;
    }

    private void Start()
    {
        BuildRuntimeDictionary();
    }

    public void BuildRuntimeDictionary()
    {
        _hexByCoord.Clear();

        var allHexes = _hexContainer.GetComponentsInChildren<Hex>();

        var lookup = allHexes.ToDictionary(h => h.Coord, h => h);

        foreach (var coord in _hexGridData.Coords)
        {
            if (lookup.TryGetValue(coord, out var hex))
                _hexByCoord[coord] = hex;
            else
                Debug.LogWarning($"HexGrid: не найден Hex с Coord {coord}", this);
        }

        Debug.Log($"HexGrid: загружено {_hexByCoord.Count}/{_hexGridData.Coords.Count} тайлов.");
    }

    public Hex GetRandomHex()
    {
        if (_hexByCoord.Count == 0)
            return null;

        var keys = _hexByCoord.Keys.ToList();
        var randomKey = keys[Random.Range(0, keys.Count)];
        return _hexByCoord[randomKey];
    }

    public IEnumerable<Hex> GetNeighbors(AxialCoord coord)
    {
        foreach (var (dq, dr) in AxialCoord.Directions)
        {
            var neighbor = new AxialCoord(coord.Q + dq, coord.R + dr);

            if (_hexByCoord.TryGetValue(neighbor, out var hex))
                yield return hex;
        }
    }

    public void Reset()
    {
        foreach (var hex in _hexByCoord.Values)
            hex.Reset();   
        
        _hexByCoord.Clear();
    }
}
