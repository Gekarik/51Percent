using System.Collections.Generic;
using UnityEngine;

public interface IHexGridProvider
{
    int Count { get; }
    IReadOnlyList<IHex> AllHexes { get; }

    IHex GetHex(HexCoord coord);
    bool TryGetHex(HexCoord coord, out IHex hex);
    IHex GetHexAt(Vector3 worldPosition);
    HexCoord GetCoord(IHex hex);

    IEnumerable<IHex> GetNeighbors(HexCoord coord);
    IEnumerable<IHex> GetNeighbors(IHex hex);
    int CalculateDistance(HexCoord a, HexCoord b);
    int CalculateDistance(IHex a, IHex b);

    IHex GetRandomHex();
    IEnumerable<IHex> GetHexesInRadius(HexCoord center, int radius);
}
