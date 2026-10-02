using System;
using System.Collections.Generic;
using UnityEngine;

// Правила трейла: что происходит при входе персонажа на клетку, когда трейл закрывается
// захватом и как он возвращается прежним владельцам. Обычный объект — положение персонажа
// и кадры сюда не попадают, их отслеживает Conqueror и сообщает готовую клетку.
public class TrailRun
{
    private readonly ITerritoryWriter _territory;
    private readonly IHexGridProvider _grid;
    private readonly ICharacter _owner;
    private readonly CharacterStats _stats;

    private readonly List<IHex> _trail = new List<IHex>();
    private readonly Dictionary<IHex, ICharacter> _previousOwners = new Dictionary<IHex, ICharacter>();
    private readonly List<IHex> _releaseBuffer = new List<IHex>();
    private readonly HashSet<ICharacter> _affectedBuffer = new HashSet<ICharacter>();

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;

    public TrailRun(ITerritoryWriter territory, IHexGridProvider grid, ICharacter owner, CharacterStats stats)
    {
        _territory = territory ?? throw new ArgumentNullException(nameof(territory));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
    }

    public IReadOnlyList<IHex> Hexes => _trail;
    public IReadOnlyCollection<IHex> FixedHexes => _territory.GetFixedByOwner(_owner);

    // Клетка, на которой персонаж находится по мнению правил. Нужна, чтобы отличить
    // «мы всё ещё здесь» от «нас унесло крыльями посреди обработки»
    public IHex CurrentHex { get; private set; }

    public void SetStartHex(IHex hex) => CurrentHex = hex;

    // Трейл оторван, если потеряна связь с собственной территорией: возвращаться некуда
    public bool IsOrphaned()
    {
        if (FixedHexes.Count == 0)
            return true;

        foreach (var trailHex in _trail)
            foreach (var neighbor in _grid.GetNeighbors(trailHex))
                if (_territory.IsFixedBy(_owner, neighbor))
                    return false;

        return true;
    }

    // Единственная операция входа на клетку: дальше всё решают правила над данными поля
    public void Enter(IHex hex)
    {
        CurrentHex = hex;

        if (hex.State == HexState.PartOfTrail && hex.Owner != _owner)
        {
            TrailInterrupted?.Invoke(hex.Owner, _owner);

            if (_owner.CanAct && CurrentHex == hex)
                AddToTrail(hex);

            return;
        }

        if (!_territory.IsFixedBy(_owner, hex) || hex.Owner != _owner)
            AddToTrail(hex);
        else if (_trail.Count > 0 && hex.State == HexState.Busy && hex.Owner == _owner)
            CloseTrail();
    }

    // Бросить трейл без потери территории: клетки возвращаются прежним владельцам
    public void Abandon()
    {
        using var changes = _territory.BeginChanges();
        RestoreTrailHexes();
        Clear();
        CurrentHex = null;
    }

    public void ReleaseTerritory()
    {
        using var changes = _territory.BeginChanges();
        RestoreTrailHexes();
        ReleaseOwnFixedHexes();
        _territory.OnCharacterDied(_owner);
        Clear();
        CurrentHex = null;
    }

    private void AddToTrail(IHex hex)
    {
        AddSingleHex(hex);

        int radius = Mathf.Max(0, Mathf.RoundToInt(_stats.GetValue(StatType.CaptureWidth)) - 1);
        if (radius == 0)
            return;

        var coord = _grid.GetCoord(hex);
        foreach (var neighbour in _grid.GetHexesInRadius(coord, radius))
        {
            if (neighbour.State == HexState.Busy && neighbour.Owner == _owner)
                continue;

            // Широкий захват режет чужой трейл по тем же правилам, что и прямое пересечение
            if (neighbour.State == HexState.PartOfTrail && neighbour.Owner != _owner)
            {
                TrailInterrupted?.Invoke(neighbour.Owner, _owner);

                // Шипы могут убить нас, а крылья — перенести с обрабатываемой клетки
                if (!_owner.CanAct || CurrentHex != hex)
                    return;
            }

            AddSingleHex(neighbour);
        }
    }

    private void AddSingleHex(IHex hex)
    {
        if (_trail.Contains(hex))
            return;

        _previousOwners[hex] = hex.Owner;
        _trail.Add(hex);
        _territory.TrailHex(_owner, hex);
    }

    private void CloseTrail()
    {
        IReadOnlyList<IHex> captured;
        using (_territory.BeginChanges())
        {
            captured = _territory.CaptureEnclosedArea(_owner, _trail);
            Clear();
        }

        AreaCaptured?.Invoke(_owner, captured);
    }

    private void Clear()
    {
        _trail.Clear();
        _previousOwners.Clear();
    }

    private void RestoreTrailHexes()
    {
        _affectedBuffer.Clear();

        foreach (var hex in _trail)
        {
            if (hex.Owner != null && hex.Owner != _owner)
                continue;

            if (_previousOwners.TryGetValue(hex, out var previousOwner)
                && previousOwner != null
                && previousOwner.State == CharacterState.Alive)
            {
                var currentOwner = hex.Owner;
                if (currentOwner != null && currentOwner != previousOwner && currentOwner != _owner)
                    _affectedBuffer.Add(currentOwner);

                _territory.FixHex(previousOwner, hex);
            }
            else
            {
                _territory.ReleaseHex(hex);
            }
        }

        foreach (var character in _affectedBuffer)
            _territory.ReleaseDisconnectedFragments(character);
    }

    private void ReleaseOwnFixedHexes()
    {
        _releaseBuffer.Clear();

        foreach (var hex in FixedHexes)
            _releaseBuffer.Add(hex);

        foreach (var hex in _releaseBuffer)
            hex.Reset();
    }
}
