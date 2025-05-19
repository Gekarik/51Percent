using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour
{
    private readonly Dictionary<AxialCoord, Hex> _hexByCoord = new Dictionary<AxialCoord, Hex>();

    public IEnumerable<Hex> GetAllHexes() => _hexByCoord.Values;

    public void BuildRuntimeDictionary(Transform container, HexGridData hexGridData)
    {
        _hexByCoord.Clear();
        var coords = hexGridData.Coords;

        int index = 0;
        foreach (Transform child in container)
        {
            if (child.TryGetComponent<Hex>(out var hex))
            {
                if (index < coords.Count)
                {
                    var coord = coords[index];
                    hex.SetCoord(coord);
                    _hexByCoord[coord] = hex;
                    index++;
                }
                else
                {
                    Debug.LogWarning("HexGridData has fewer coordinates than Hex objects.");
                    break;
                }
            }
        }

        if (index < coords.Count)
        {
            Debug.LogWarning("Some coordinates in HexGridData were not assigned to any Hex.");
        }

        Debug.Log(_hexByCoord.Count.ToString());
    }

    public Hex GetRandomHex() => _hexByCoord.ElementAt(Random.Range(0, _hexByCoord.Count)).Value;
    public int GetCount() => _hexByCoord.Count;
    public Hex GetHex(AxialCoord coord) => _hexByCoord.TryGetValue(coord, out var hex) ? hex : null;
    public AxialCoord GetCoord(Hex hex) => _hexByCoord.FirstOrDefault(x => x.Value == hex).Key;

    public IEnumerable<Hex> GetNeighbors(Hex hex)
    {
        foreach (var (dq, dr) in AxialCoord.Directions)
        {
            var c = new AxialCoord(hex.Coord.Q + dq, hex.Coord.R + dr);
            if (_hexByCoord.TryGetValue(c, out var neighbor))
                yield return neighbor;
        }
    }
}
