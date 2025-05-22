using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    [SerializeField] private HexGrid _grid;
    [SerializeField] private PolygonZoneTester _zoneTester;

    public Action<ICharacter, ICharacter> TrailInterrupted;

    private ICharacter _player;

    private readonly HashSet<Hex> _trailSet = new HashSet<Hex>();
    private readonly HashSet<Hex> _fixedSet = new HashSet<Hex>();
    private readonly List<Vector2> _polyCache = new List<Vector2>();

    private void Awake()
    {
        _player = GetComponent<ICharacter>();
        CreateZoneTester();
    }

    private void CreateZoneTester()
    {
        var zoneObject = new GameObject("ZoneTester");
        zoneObject.transform.SetParent(transform);
        _zoneTester = zoneObject.AddComponent<PolygonZoneTester>();
    }

    public void Init(HexGrid grid)
    {
        _grid = grid;
    }

    public void GetStartTerritory(Hex startHex)
    {
        var hexes = _grid.GetNeighborsCached(startHex).Append(startHex);

        foreach (var hex in hexes)
            CaptureHex(hex);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Hex hex) == false || hex.Owner == _player)
            return;

        if (hex.Owner != _player && hex.State != HexState.PartOfTrail)
        {
            if (_trailSet.Add(hex))
                hex.AddedToTrail();

            return;
        }

        if (_trailSet.Count > 0 && hex.Owner == _player)
        {
            FixTerritory();
            _trailSet.Clear();
        }
    }

    private void FixTerritory()
    {
        foreach (var hex in _trailSet)
            CaptureHex(hex);

        _polyCache.Clear();
        _polyCache.AddRange(_trailSet.Select(hex => (Vector2)hex.transform.position));

        _zoneTester.SetPolygon(_polyCache);

        foreach (var tile in _grid.AllHexes)
        {
            if (_fixedSet.Contains(tile))
                continue;

            if (_zoneTester.IsInside((Vector2)tile.transform.position))
                CaptureHex(tile);
        }
    }


    private void CaptureHex(Hex tile)
    {
        bool addedToFixed = _fixedSet.Add(tile);

        if (addedToFixed)
            tile.SetOwner(_player);
    }

    private void OnDestroy()
    {
        // zoneTester живёт на сцене, не удаляем здесь
    }
}