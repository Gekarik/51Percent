using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Hex/HexGridData")]
public class HexGridData : ScriptableObject
{
    public List<AxialCoord> Coords = new List<AxialCoord>();

    public void Clear() => Coords.Clear();
    public void Add(AxialCoord coord) => Coords.Add(coord);
}