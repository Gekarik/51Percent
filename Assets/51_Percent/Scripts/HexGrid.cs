using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour, IHexGridProvider
{
    private List<Hex> _allHexes;
    private Dictionary<IHex, List<IHex>> _neighborMap;
    private float _cellDiameter;

    public IReadOnlyList<Hex> AllHexes => _allHexes;
    public float CellDiameter => _cellDiameter;

    private void Awake()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();

        if (_allHexes.Count == 0)
            throw new InvalidOperationException("HexGrid: нет ни одного Hex в дочерних объектах");

        _cellDiameter = _allHexes[0].GetRendererBounds().size.x;
        BuildNeighborMap();
    }

    private void BuildNeighborMap()
    {
        _neighborMap = new Dictionary<IHex, List<IHex>>(_allHexes.Count);
        float radius = _cellDiameter * 1.01f;

        foreach (var hex in _allHexes)
        {
            List<IHex> neighbors = new List<IHex>(_allHexes
                .Where(h => h != hex && Vector3.Distance(hex.transform.position, h.transform.position) <= radius)
                .ToList());

            _neighborMap[hex] = neighbors;
        }
    }

    public IEnumerable<IHex> GetNeighbors(IHex hex)
    {
        return _neighborMap.TryGetValue(hex, out var list) ? list : Enumerable.Empty<IHex>();
    }

    public Hex GetRandomHex()
    {
        if (_allHexes.Count == 0)
            throw new ArgumentException("_allhexes is empty");

        return _allHexes[UnityEngine.Random.Range(0, _allHexes.Count)];
    }

    
    
    public int Distance(IHex a, Hex b)
    {
        float d = Vector3.Distance(a.Transform.position, b.transform.position);

        return Mathf.RoundToInt(d / (_cellDiameter * 1.01f));
    }
}
