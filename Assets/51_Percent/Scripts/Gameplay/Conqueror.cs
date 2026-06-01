using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ICharacter))]
public class Conqueror : MonoBehaviour
{
    private readonly List<IHex> _trailList = new List<IHex>();
    private readonly Dictionary<IHex, ICharacter> _trailPrevOwners = new Dictionary<IHex, ICharacter>();
    private readonly List<IHex> _capturedBuffer = new List<IHex>();
    private readonly List<IHex> _holesBuffer = new List<IHex>();
    private readonly List<Transform> _viewsBuffer = new List<Transform>();
    private readonly List<IHex> _resetBuffer = new List<IHex>();
    private readonly HashSet<ICharacter> _affectedBuffer = new HashSet<ICharacter>();
    private readonly HashSet<ICharacter> _resetAffectedBuffer = new HashSet<ICharacter>();
    private static readonly IHex[] _emptyTrail = System.Array.Empty<IHex>();
    private TerritoryManager _territoryManager;

    private ConquestAlgorithm _algorithm;

    public event Action<ICharacter, ICharacter> TrailInterrupted;
    public event Action<ICharacter> TrailOrphaned;
    public event Action<IReadOnlyList<Transform>> AreaCaptured;

    public IReadOnlyCollection<IHex> FixedHexes => _territoryManager.GetFixedByOwner(_owner);
    public IReadOnlyList<IHex> TrailHexes => _trailList;

    private IHexGridProvider _grid;
    private ICharacter _owner;
    private IHex _currentHex;
    private CharacterStats _stats;

    private void Awake()
    {
        _algorithm = new ConquestAlgorithm();
        _owner = GetComponent<ICharacter>();
    }

    public void Init(TerritoryManager territoryManager, IHexGridProvider grid, CharacterStats stats)
    {
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        _territoryManager.InitCharacter(_owner);
        _currentHex = _grid.GetHexAt(transform.position);

        AreaCaptured += _territoryManager.OnAreaCaptured;
    }

    private void OnDestroy()
    {
        if (_territoryManager != null)
            AreaCaptured -= _territoryManager.OnAreaCaptured;
    }

    private void FixedUpdate()
    {
        if (_grid == null || _owner == null || _owner.State != CharacterState.Alive)
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

            if (_owner.State != CharacterState.Died)
                AddToTrail(hex);

            return;
        }

        if (!_territoryManager.IsFixedBy(_owner, hex) || hex.Owner != _owner)
            AddToTrail(hex);
        else if (_trailList.Count > 0 && hex.State == HexState.Busy && hex.Owner == _owner)
            CloseTrail(hex);
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

    private void CloseTrail(IHex returnHex)
    {
        _algorithm.ComputeCapturedArea(FixedHexes, _trailList, _grid, _capturedBuffer);
        CollectAffectedCharacters(_capturedBuffer);
        CaptureAll(_capturedBuffer);
        ReleaseDisconnectedFragments();
        FillHoles();
        CollectViewTransforms();
        AreaCaptured?.Invoke(_viewsBuffer);
        ClearTrail();
    }

    private void CollectAffectedCharacters(List<IHex> hexes)
    {
        _affectedBuffer.Clear();
        foreach (var h in hexes)
        {
            if (h.Owner != null && h.Owner != _owner)
                _affectedBuffer.Add(h.Owner);

            // Захват пустого хекса может разрезать территорию соседних персонажей
            foreach (var neighbor in _grid.GetNeighbors(h))
                if (neighbor.Owner != null && neighbor.Owner != _owner)
                    _affectedBuffer.Add(neighbor.Owner);
        }
    }

    private void CaptureAll(List<IHex> hexes)
    {
        foreach (var h in hexes)
            CaptureHex(h);
    }

    private void ReleaseDisconnectedFragments()
    {
        foreach (var character in _affectedBuffer)
            _territoryManager.ReleaseDisconnectedFragments(character);
    }

    private void CollectViewTransforms()
    {
        _viewsBuffer.Clear();
        foreach (var h in _capturedBuffer)
        {
            if (h.ViewTransform != null && !_viewsBuffer.Contains(h.ViewTransform))
                _viewsBuffer.Add(h.ViewTransform);
        }
    }

    private void ClearTrail()
    {
        _trailList.Clear();
        _trailPrevOwners.Clear();
    }

    private void FillHoles()
    {
        // Запускаем до сходимости: каждый новый захват может создать новые замкнутые области
        while (true)
        {
            _algorithm.ComputeCapturedArea(FixedHexes, _emptyTrail, _grid, _holesBuffer);
            if (_holesBuffer.Count == 0) break;
            CollectAffectedCharacters(_holesBuffer);
            CaptureAll(_holesBuffer);
            ReleaseDisconnectedFragments();
        }
    }

    private void CaptureHex(IHex hex)
    {
        _territoryManager.FixHex(_owner, hex);
    }

    public void FixHexes(List<IHex> hexes)
    {
        foreach (var h in hexes)
            CaptureHex(h);
    }

    public void Reset()
    {
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
