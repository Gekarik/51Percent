using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conqueror : MonoBehaviour
{
    private readonly List<IHex> _trailList = new List<IHex>();
    private TerritoryManager _territoryManager;

    private ConquestAlgorithm _algorithm;

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<IReadOnlyList<Transform>> AreaCaptured;

    public IReadOnlyCollection<IHex> FixedHexes => _territoryManager.GetFixedByOwner(_owner);

    private IHexGridProvider _grid;
    private ICharacter _owner;

    private void Awake()
    {
        _algorithm = new ConquestAlgorithm();
        _owner = GetComponent<ICharacter>();
    }

    public void Init(TerritoryManager territoryManager, IHexGridProvider grid)
    {
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _territoryManager.InitCharacter(_owner);

        AreaCaptured += _territoryManager.OnAreaCaptured;
    }

    private void OnDestroy()
    {
        if (_territoryManager != null)
            AreaCaptured -= _territoryManager.OnAreaCaptured;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_owner == null || _owner.State != CharacterState.Alive)
            return;

        if (collision.gameObject.TryGetComponent(out IHex hex) == false)
            return;

        if (hex.State == HexState.PartOfTrail && hex.Owner != _owner)
        {
            TrailInterrupted?.Invoke(hex.Owner, _owner);
            AddToTrail(hex);

            return;
        }

        if (FixedHexes.Contains(hex) == false || hex.Owner != _owner)
            AddToTrail(hex);
        else if (_trailList.Count > 0 && hex.State == HexState.Busy && hex.Owner == _owner)
            CloseTrail(hex);
    }

    private void AddToTrail(IHex hex)
    {
        if (!_trailList.Contains(hex))
        {
            _trailList.Add(hex);
            hex.SetOwner(_owner, HexState.PartOfTrail);
        }
    }

    private void CloseTrail(IHex returnHex)
    {
        if (!_trailList.Contains(returnHex))
            _trailList.Add(returnHex);

        var captured = _algorithm.ComputeCapturedArea(FixedHexes, _trailList, _grid);

        foreach (var h in captured)
            CaptureHex(h);

        var views = captured.Where(h => h.HexView != null).Select(h => h.HexView.transform).Distinct().ToList();

        AreaCaptured?.Invoke(views);
        _trailList.Clear();
    }

    private void CaptureHex(IHex hex)
    {
        _territoryManager.FixHex(_owner, hex);
        hex.SetOwner(_owner, HexState.Busy);
    }

    public void FixHexes(List<IHex> hexes)
    {
        foreach (var h in hexes)
            CaptureHex(h);
    }

    public void Reset()
    {
        var hexesToReset = _trailList.Concat(FixedHexes).ToList();

        foreach (var hex in hexesToReset)
            hex.Reset();

        _territoryManager?.OnCharacterDied(_owner);
        _trailList.Clear();
    }
}
