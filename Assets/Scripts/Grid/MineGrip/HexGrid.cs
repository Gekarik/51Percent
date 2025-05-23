using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("Neighbors Detection")]
    [Tooltip("Радиус окружности для поиска соседей вокруг центра Hex")]

    private List<Hex> _allHexes;
    private Dictionary<AxialCoord, Hex> _coordToHex = new();
    private readonly Dictionary<Hex, IReadOnlyList<Hex>> _neighborsCache = new();

    private void Awake()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();
        _coordToHex.Clear();

        foreach (var hex in _allHexes)
            _coordToHex[hex.Coord] = hex;
    }

    /// <summary>Все Hex в этой сетке.</summary>
    public IReadOnlyList<Hex> AllHexes => _allHexes;

    /// <summary>Возвращает рандомный Hex из сетки или null, если сетка пуста.</summary>
    public Hex GetRandomHex()
    {
        if (_allHexes.Count == 0)
            return null;
        int idx = UnityEngine.Random.Range(0, _allHexes.Count);
        return _allHexes[idx];
    }

    /// <summary>Возвращает кэшированных соседей через AxialCoord.</summary>
    public IReadOnlyList<Hex> GetNeighborsCached(Hex tile)
    {
        if (_neighborsCache.TryGetValue(tile, out var cached))
            return cached;

        var coord = tile.Coord;
        var result = new List<Hex>();

        foreach (var (dq, dr) in AxialCoord.Directions)
        {
            var neighborCoord = new AxialCoord(coord.Q + dq, coord.R + dr);
            if (_coordToHex.TryGetValue(neighborCoord, out var neighbor))
                result.Add(neighbor);
        }

        _neighborsCache[tile] = result;
        return result;
    }

    /// <summary>Проверяет наличие Hex по координатам.</summary>
    public bool TryGetHex(AxialCoord coord, out Hex hex)
    {
        return _coordToHex.TryGetValue(coord, out hex);
    }

    /// <summary>Возвращает соседей по координатам без кэша.</summary>
    public IReadOnlyList<Hex> GetLogicalNeighbors(Hex hex)
    {
        var neighbors = new List<Hex>(6);
        var center = hex.Coord;

        foreach (var offset in AxialCoord.Directions)
        {
            var coord = new AxialCoord(center.Q + offset.dq, center.R + offset.dr);
            if (_coordToHex.TryGetValue(coord, out var neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    /// <summary>При изменении числа Hex (например, регенерации), очистить кэш и пересобрать список.</summary>
    public void Rebuild()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();
        _coordToHex.Clear();
        _neighborsCache.Clear();

        foreach (var hex in _allHexes)
            _coordToHex[hex.Coord] = hex;
    }
}
