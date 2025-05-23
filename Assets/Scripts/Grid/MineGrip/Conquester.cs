using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    [SerializeField] private HexGrid _grid;

    public Action<ICharacter, ICharacter> TrailInterrupted;

    private ICharacter _player;
    private readonly List<Hex> _trailList = new List<Hex>();
    private readonly HashSet<Hex> _fixed = new HashSet<Hex>();

    private void Awake()
    {
        _player = GetComponent<ICharacter>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out Hex hex))
            return;

        switch (hex.State)
        {
            case HexState.PartOfTrail when hex.Owner != _player:
                TrailInterrupted?.Invoke(hex.Owner, _player);
                break;

            case HexState.Busy when hex.Owner == _player && _trailList.Count > 0:
                HandleClosure(hex);
                break;

            case HexState.Empty:
                HandleTrail(hex);
                break;
        }
    }

    private void HandleTrail(Hex hex)
    {
        if (_trailList.Contains(hex) == false)
        {
            _trailList.Add(hex);
            hex.SetOwner(_player);
            hex.SetState(HexState.PartOfTrail);
        }
    }

    private void HandleClosure(Hex returnHex)
    {
        if (_trailList.Contains(returnHex) == false)
            _trailList.Add(returnHex);

        foreach (var t in _trailList)
            CaptureHex(t);

        var poly = _trailList
            .Select(h => (x: h.transform.position.x, y: h.transform.position.z))
            .ToList();

        foreach (var hex in _grid.AllHexes)
        {
            if (_fixed.Contains(hex))
                continue;

            var pt = (x: hex.transform.position.x, y: hex.transform.position.z);
            if (PointInPolygon(pt, poly))
                CaptureHex(hex);
        }

        _trailList.Clear();
    }

    private bool PointInPolygon((float x, float y) p, List<(float x, float y)> poly)
    {
        bool inside = false;
        int n = poly.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var xi = poly[i].x; var yi = poly[i].y;
            var xj = poly[j].x; var yj = poly[j].y;

            bool intersect = ((yi > p.y) != (yj > p.y))
                             && (p.x < (xj - xi) * (p.y - yi) / (yj - yi + float.Epsilon) + xi);
            if (intersect)
                inside = !inside;
        }
        return inside;
    }

    private void CaptureHex(Hex h)
    {
        if (_fixed.Add(h))
        {
            h.SetOwner(_player);
            h.SetState(HexState.Busy);
        }
    }

    public void Init(HexGrid grid) => _grid = grid ?? throw new ArgumentNullException();
    public void GetStartTerritory(Hex start)
    {
        foreach (var h in _grid.GetNeighborsCached(start).Append(start))
            CaptureHex(h);
    }
    public void Reset()
    {
        foreach (var h in _fixed) h.Reset();
        _fixed.Clear();
        _trailList.Clear();
    }
}
