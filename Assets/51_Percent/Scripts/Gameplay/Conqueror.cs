using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conqueror : MonoBehaviour
{
    private readonly List<IHex> _trailList = new List<IHex>();
    private readonly Dictionary<IHex, ICharacter> _trailPrevOwners = new Dictionary<IHex, ICharacter>();
    private readonly List<IHex> _resetBuffer = new List<IHex>();
    private readonly HashSet<ICharacter> _resetAffectedBuffer = new HashSet<ICharacter>();
    private TerritoryManager _territoryManager;

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<ICharacter> TrailOrphaned;
    public event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;

    public IReadOnlyCollection<IHex> FixedHexes => _territoryManager.GetFixedByOwner(_owner);
    public IReadOnlyList<IHex> TrailHexes => _trailList;

    private IHexGridProvider _grid;
    private ICharacter _owner;
    private IHex _currentHex;
    private CharacterStats _stats;

    private void Awake()
    {
        _owner = GetComponent<ICharacter>();
    }

    public void Init(TerritoryManager territoryManager, IHexGridProvider grid, CharacterStats stats)
    {
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        _currentHex = _grid.GetHexAt(transform.position);
    }

    private void FixedUpdate()
    {
        if (_grid == null || _owner == null || !_owner.CanAct)
            return;

        if (_trailList.Count > 0 && IsTrailOrphaned())
        {
            TrailOrphaned?.Invoke(_owner);
            return;
        }

        var hex = _grid.GetHexAt(transform.position);

        if (hex == null || hex == _currentHex)
            return;

        _currentHex = hex;
        OnHexEntered(hex);
    }

    private bool IsTrailOrphaned()
    {
        if (FixedHexes.Count == 0)
            return true;

        foreach (var trailHex in _trailList)
            foreach (var neighbor in _grid.GetNeighbors(trailHex))
                if (_territoryManager.IsFixedBy(_owner, neighbor))
                    return false;

        return true;
    }

    private void OnHexEntered(IHex hex)
    {
        if (hex.State == HexState.PartOfTrail && hex.Owner != _owner)
        {
            TrailInterrupted?.Invoke(hex.Owner, _owner);

            if (_owner.CanAct && _currentHex == hex)
                AddToTrail(hex);

            return;
        }

        if (!_territoryManager.IsFixedBy(_owner, hex) || hex.Owner != _owner)
            AddToTrail(hex);
        else if (_trailList.Count > 0 && hex.State == HexState.Busy && hex.Owner == _owner)
            CloseTrail();
    }

    private void AddToTrail(IHex hex)
    {
        AddSingleHexToTrail(hex);

        int radius = Mathf.Max(0, Mathf.RoundToInt(_stats.GetValue(StatType.CaptureWidth)) - 1);
        if (radius == 0) return;

        var coord = _grid.GetCoord(hex);
        foreach (var h in _grid.GetHexesInRadius(coord, radius))
        {
            if (h.State == HexState.Busy && h.Owner == _owner) continue;

            // Широкий захват режет чужой трейл по тем же правилам, что и прямое пересечение
            if (h.State == HexState.PartOfTrail && h.Owner != _owner)
            {
                TrailInterrupted?.Invoke(h.Owner, _owner);

                // Шипы могут убить нас, а крылья — перенести с обрабатываемой клетки.
                if (!_owner.CanAct || _currentHex != hex)
                    return;
            }

            AddSingleHexToTrail(h);
        }
    }

    private void AddSingleHexToTrail(IHex hex)
    {
        if (_trailList.Contains(hex)) return;
        _trailPrevOwners[hex] = hex.Owner;
        _trailList.Add(hex);
        _territoryManager.TrailHex(_owner, hex);
    }

    private void CloseTrail()
    {
        IReadOnlyList<IHex> captured;
        using (_territoryManager.BeginChanges())
        {
            captured = _territoryManager.CaptureEnclosedArea(_owner, _trailList);
            ClearTrail();
        }
        AreaCaptured?.Invoke(_owner, captured);
    }

    private void ClearTrail()
    {
        _trailList.Clear();
        _trailPrevOwners.Clear();
    }

    // Бросить трейл без потери территории: гексы возвращаются прежним владельцам
    public void AbandonTrail()
    {
        using var changes = _territoryManager.BeginChanges();
        RestoreTrailHexes();
        ClearTrail();
        _currentHex = null;
    }

    public void ReleaseTerritory()
    {
        using var changes = _territoryManager.BeginChanges();
        RestoreTrailHexes();
        ReleaseOwnFixedHexes();
        _territoryManager?.OnCharacterDied(_owner);
        ClearTrail();
        _currentHex = null;
    }

    private void RestoreTrailHexes()
    {
        _resetAffectedBuffer.Clear();
        foreach (var hex in _trailList)
        {
            if (hex.Owner != null && hex.Owner != _owner)
                continue;

            if (_trailPrevOwners.TryGetValue(hex, out var prevOwner)
                && prevOwner != null
                && prevOwner.State == CharacterState.Alive)
            {
                var currentOwner = hex.Owner;
                if (currentOwner != null && currentOwner != prevOwner && currentOwner != _owner)
                    _resetAffectedBuffer.Add(currentOwner);

                _territoryManager.FixHex(prevOwner, hex);
            }
            else
            {
                _territoryManager.ReleaseHex(hex);
            }
        }

        foreach (var character in _resetAffectedBuffer)
            _territoryManager.ReleaseDisconnectedFragments(character);
    }

    private void ReleaseOwnFixedHexes()
    {
        _resetBuffer.Clear();
        foreach (var hex in FixedHexes)
            _resetBuffer.Add(hex);
        foreach (var hex in _resetBuffer)
            hex.Reset();
    }
}
