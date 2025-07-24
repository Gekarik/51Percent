using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    private KillManager _killManager;
    private ConquestAlgorithm _algorithm;
    private readonly List<Hex> _trailList = new List<Hex>();
    private readonly HashSet<Hex> _fixed = new HashSet<Hex>();

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<IReadOnlyList<Transform>> AreaCaptured;
    public IReadOnlyCollection<Hex> FixedHexes => _fixed;

    private IHexGridProvider _grid;
    private ICharacter _owner;

    private void Awake()
    {
        _algorithm = new ConquestAlgorithm();
        _owner = GetComponent<ICharacter>();
        _killManager = new KillManager(this);
    }

    public void Init(IHexGridProvider grid)
    {
        _grid = grid ?? throw new ArgumentException();
        
        AreaCaptured += _grid.OnAreaCaptured;
    }

    private void OnDisable()
    {
        AreaCaptured -= _grid.OnAreaCaptured;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out Hex hex))
            return;

        if (hex.State == HexState.PartOfTrail && hex.Owner != _owner)
        {
            TrailInterrupted?.Invoke(hex.Owner, _owner);
            AddToTrail(hex);
            return;
        }

        if (_fixed.Contains(hex) == false)
            AddToTrail(hex);
        else if (_trailList.Count > 0 && hex.State == HexState.Busy && hex.Owner == _owner)
            CloseTrail(hex);
    }

    public void GetStartTerritory(Hex starthex)
    {
        var hexes = _grid.GetNeighbors(starthex).Append(starthex);
        FixHexes(hexes);
    }

    private void AddToTrail(Hex hex)
    {
        if (!_trailList.Contains(hex))
        {
            _trailList.Add(hex);
            hex.SetOwner(_owner, HexState.PartOfTrail);
        }
    }

    private void CloseTrail(Hex returnHex)
    {
        if (!_trailList.Contains(returnHex))
            _trailList.Add(returnHex);

        var captured = _algorithm.ComputeCapturedArea(_fixed, _trailList, _grid);

        foreach (var h in captured)
            CaptureHex(h);

        var views = captured.Where(h => h.HexView != null).Select(h => h.HexView.transform).Distinct().ToList();

        AreaCaptured?.Invoke(views);
        _trailList.Clear();
    }

    private void CaptureHex(Hex hex)
    {
        if (_fixed.Add(hex))
            hex.SetOwner(_owner, HexState.Busy);
    }

    public void FixHexes(IEnumerable<Hex> hexes)
    {
        foreach (var h in hexes)
            CaptureHex(h);
    }

    public void Reset()
    {
        foreach (var h in _fixed)
            h.Reset();

        foreach (var h in _trailList)
            h.Reset();

        _fixed.Clear();
        _trailList.Clear();
    }
}
