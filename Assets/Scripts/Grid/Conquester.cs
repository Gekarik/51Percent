using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    public Action<ICharacter, ICharacter> TrailInterrupted;
    public Action<IReadOnlyList<Transform>> AreaCaptured;

    private IHexGridProvider _grid;
    private ICharacter _player;

    private readonly HashSet<Hex> _trailList = new HashSet<Hex>();
    private readonly HashSet<Hex> _fixed = new HashSet<Hex>();

    private void Awake()
    {
        _player = GetComponent<ICharacter>();
    }

    public void Init(HexGrid grid)
    {
        _grid = grid ?? throw new ArgumentNullException();
        AreaCaptured += _grid.OnAreaCaptured;
    }

    private void OnDisable()
    {
        AreaCaptured -= _grid.OnAreaCaptured;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Hex hex) == false)
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
            hex.SetOwner(_player, HexState.PartOfTrail);
        }
    }

    private void HandleClosure(Hex returnHex)
    {
        if (_trailList.Contains(returnHex) == false)
            _trailList.Add(returnHex);

        var toCapture = new ConquestAlgorithm().ComputeCapturedArea(_fixed, _trailList, _grid);

        foreach (var hex in toCapture)
            CaptureHex(hex);

        var transforms = toCapture
            .Where(h => h.HexView != null)
            .Select(h => h.HexView.transform)
            .Distinct()
            .ToList();

        Debug.Log(transforms.Count);
        AreaCaptured?.Invoke(transforms);

        _trailList.Clear();
    }

    private void CaptureHex(Hex hex)
    {
        if (_fixed.Add(hex))
            hex.SetOwner(_player, HexState.Busy);
    }

    public void GetStartTerritory(Hex startHex)
    {
        var startTerritory = _grid.GetNeighbors(startHex).Append(startHex);

        foreach (var hex in startTerritory)
            CaptureHex(hex);
    }

    public void Reset()
    {
        foreach (var hex in _fixed)
            hex.Reset();

        _fixed.Clear();
        _trailList.Clear();
    }
}
