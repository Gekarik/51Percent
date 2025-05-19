// HexGridData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexGridData", menuName = "Hex/Grid Data")]
public class HexGridData : ScriptableObject
{
    [SerializeField] private List<AxialCoord> _coords = new List<AxialCoord>();
    public IReadOnlyList<AxialCoord> Coords => _coords;

    public void RegisterCoord(AxialCoord coord)
    {
        if (!_coords.Contains(coord))
            _coords.Add(coord);
    }

    public void UnregisterCoord(AxialCoord coord)
    {
        _coords.Remove(coord);
    }

    public void Clear()
    {
        _coords.Clear();
    }
}
