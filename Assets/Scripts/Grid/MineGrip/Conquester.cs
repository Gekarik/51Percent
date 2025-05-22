using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    public Action<ICharacter, ICharacter> TrailInterrupted;

    [SerializeField] private HexGrid _grid;

    private ICharacter _player;
    private readonly HashSet<Hex> _trailSet = new();
    private readonly HashSet<Hex> _fixedSet = new();

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
                return;

            case HexState.Busy when hex.Owner == _player && _trailSet.Count > 0:
                CaptureTerritory();
                ResetTrail();
                return;

            case HexState.Empty:
                if (_trailSet.Add(hex))
                    hex.SetState(HexState.PartOfTrail);
                return;

            default:
                return;
        }
    }


    private void CaptureTerritory()
    {
        // Сначала фиксируем все в trail
        foreach (var h in _trailSet)
            CaptureHex(h);

        // Строим полигон из trail в порядке добавления
        var poly = _trailSet
            .Select(h => new Vector2(h.transform.position.x, h.transform.position.z))
            .ToList();

        // Проверяем каждый hex вне фиксированного набора
        foreach (var tile in _grid.AllHexes)
        {
            if (_fixedSet.Contains(tile))
                continue;

            var p = new Vector2(tile.transform.position.x, tile.transform.position.z);
            if (IsPointInPolygon(p, poly))
                CaptureHex(tile);
        }
    }

    private void CaptureHex(Hex h)
    {
        if (_fixedSet.Add(h))
        {
            h.SetOwner(_player);
            h.SetState(HexState.Busy);
        }
    }

    public void GetStartTerritory(Hex startHex)
    {
        var hexes = _grid.GetNeighborsCached(startHex).Append(startHex);

        foreach (var hex in hexes)
            CaptureHex(hex);
    }

    private void ResetTrail()
    {
        _trailSet.Clear();
    }

    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        int count = polygon.Count;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];
            bool intersect = ((pi.y > point.y) != (pj.y > point.y))
                             && (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x);
            if (intersect)
                inside = !inside;
        }
        return inside;
    }

    public void Reset()
    {
        foreach (var hex in _trailSet.Concat(_fixedSet))
            hex.Reset();

        _trailSet.Clear();
        _fixedSet.Clear();
    }

    public void Init(HexGrid grid) => _grid = grid ?? throw new ArgumentNullException(nameof(grid));
}
