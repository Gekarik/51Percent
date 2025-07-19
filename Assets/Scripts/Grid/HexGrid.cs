using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour, IHexGridProvider
{
    [SerializeField] private ViewAnimator _viewAnimator;
    private List<Hex> _allHexes;
    private Dictionary<Hex, List<Hex>> _neighborMap;
    private float _cellDiameter;

    public IReadOnlyList<Hex> AllHexes => _allHexes;
    public ViewAnimator ViewAnimator => _viewAnimator;
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
        _neighborMap = new Dictionary<Hex, List<Hex>>(_allHexes.Count);
        float radius = _cellDiameter * 1.01f;
        foreach (var hex in _allHexes)
        {
            var neighbors = _allHexes
                .Where(h => h != hex && Vector3.Distance(hex.transform.position, h.transform.position) <= radius)
                .ToList();
            _neighborMap[hex] = neighbors;
        }
    }

    public IEnumerable<Hex> GetNeighbors(Hex hex)
    {
        return _neighborMap.TryGetValue(hex, out var list) ? list : Enumerable.Empty<Hex>();
    }

    public Hex GetRandomHex() => _allHexes.Count > 0 ? _allHexes[UnityEngine.Random.Range(0, _allHexes.Count)] : null;

    public void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView)
    {
        _viewAnimator?.Wave(hexesView);
        Debug.Log(hexesView.Count);
    }

    public int Distance(Hex a, Hex b)
    {
        float d = Vector3.Distance(a.transform.position, b.transform.position);
        return Mathf.RoundToInt(d / (_cellDiameter * 1.01f));
    }
}
