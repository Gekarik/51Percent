using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conqueror : MonoBehaviour, ITrailObservable
{
    private IHexGridProvider _grid;
    private ICharacter _owner;
    private TrailRun _run;

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<ICharacter> TrailOrphaned;
    public event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;

    public IReadOnlyCollection<IHex> FixedHexes => _run.FixedHexes;
    public IReadOnlyList<IHex> TrailHexes => _run.Hexes;

    public bool HasActiveTrail => _run != null && _run.Hexes.Count > 0;

    private void Awake()
    {
        _owner = GetComponent<ICharacter>();
    }

    public void Init(ITerritoryWriter territory, IHexGridProvider grid, CharacterStats stats)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));

        _run = new TrailRun(territory, grid, _owner, stats);
        _run.TrailInterrupted += OnTrailInterrupted;
        _run.AreaCaptured += OnAreaCaptured;
        _run.SetStartHex(_grid.GetHexAt(transform.position));
    }

    private void OnDestroy()
    {
        if (_run == null)
            return;

        _run.TrailInterrupted -= OnTrailInterrupted;
        _run.AreaCaptured -= OnAreaCaptured;
    }

    private void FixedUpdate()
    {
        if (_run == null || _owner == null || !_owner.CanAct)
            return;

        if (_run.Hexes.Count > 0 && _run.IsOrphaned())
        {
            TrailOrphaned?.Invoke(_owner);
            return;
        }

        var hex = _grid.GetHexAt(transform.position);

        if (hex == null || hex == _run.CurrentHex)
            return;

        _run.Enter(hex);
    }

    public void AbandonTrail() => _run.Abandon();

    public void ReleaseTerritory() => _run.ReleaseTerritory();

    private void OnTrailInterrupted(ICharacter trailOwner, ICharacter stepper)
    {
        TrailInterrupted?.Invoke(trailOwner, stepper);
    }

    private void OnAreaCaptured(ICharacter owner, IReadOnlyList<IHex> captured)
    {
        AreaCaptured?.Invoke(owner, captured);
    }
}
