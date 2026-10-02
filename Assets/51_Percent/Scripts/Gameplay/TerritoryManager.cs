using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// После HexGrid (-50), но до гексов и презентеров (0): доменный индекс должен
// подписаться на StateChanged раньше презентации, чтобы сбой вьюшки не ломал учёт владения
[DefaultExecutionOrder(-40)]
public class TerritoryManager : MonoBehaviour, ITerritoryOwnership, ITerritoryWriter
{
    [Required] [SerializeField] private HexGrid _hexGrid;

    private readonly OwnershipTracker _tracker = new OwnershipTracker();

    // Один алгоритм на всех: его кэш соседей строится по гриду, а не по персонажу
    private readonly ConquestAlgorithm _conquestAlgorithm = new ConquestAlgorithm();
    private readonly List<IHex> _capturedBuffer = new List<IHex>();
    private readonly List<IHex> _holesBuffer = new List<IHex>();
    private readonly List<IHex> _releaseBuffer = new List<IHex>();
    private readonly HashSet<ICharacter> _affectedBuffer = new HashSet<ICharacter>();
    private readonly IHex[] _emptyTrail = Array.Empty<IHex>();

    private readonly HashSet<IHex> _fragmentVisited = new HashSet<IHex>();
    private readonly Queue<IHex> _fragmentQueue = new Queue<IHex>();
    private readonly List<List<IHex>> _components = new List<List<IHex>>();
    private readonly List<IHex> _componentBuffer = new List<IHex>();

    public event Action OwnershipChanged
    {
        add => _tracker.OwnershipChanged += value;
        remove => _tracker.OwnershipChanged -= value;
    }

    private void Awake()
    {
        if (_hexGrid == null)
            throw new NullReferenceException(nameof(_hexGrid));

        _tracker.Initialize(_hexGrid.AllHexes);
    }

    public IDisposable BeginChanges() => _tracker.BeginChanges();

    private void OnDestroy()
    {
        _tracker.Dispose();
    }

    public void GetStartTerritory(ICharacter character, IHex startHex)
    {
        IEnumerable<IHex> hexes = _hexGrid.GetNeighbors(startHex).Append(startHex);
        FixHexes(character, hexes);
    }

    public IReadOnlyCollection<IHex> GetFixedByOwner(ICharacter character) => _tracker.GetOwned(character);

    public bool IsFixedBy(ICharacter character, IHex hex) => _tracker.IsOwnedBy(character, hex);

    public float GetOwnershipPercent(ICharacter character)
    {
        int totalHexes = _hexGrid.AllHexes.Count;
        if (totalHexes == 0)
            return 0f;

        return (float)_tracker.GetOwned(character).Count / totalHexes;
    }

    public void OnCharacterDied(ICharacter character)
    {
        if (character == null)
            return;

        using var changes = BeginChanges();
        var owned = _tracker.GetOwned(character);
        if (owned.Count == 0)
            return;

        // Копия: SetOwner через событие меняет тот же самый набор
        _releaseBuffer.Clear();
        _releaseBuffer.AddRange(owned);

        foreach (var hex in _releaseBuffer)
            ReleaseHex(hex);
    }

    public void FixHexes(ICharacter character, IEnumerable<IHex> hexes)
    {
        using var changes = BeginChanges();
        foreach (var h in hexes)
            FixHex(character, h);
    }

    // Владение хранит сам гекс, поэтому запись идёт в него напрямую.
    // Индекс владельцев обновится сам — трекер подписан на StateChanged
    public void FixHex(ICharacter character, IHex hex)
    {
        hex?.SetOwner(character, HexState.Busy);
    }

    public void ReleaseHex(IHex hex)
    {
        hex?.SetOwner(null, HexState.Empty);
    }

    public void TrailHex(ICharacter character, IHex hex)
    {
        hex?.SetOwner(character, HexState.PartOfTrail);
    }

    // Захватывает область, ограниченную закреплённой территорией и трейлом.
    // Возвращённый список валиден до следующего вызова
    public IReadOnlyList<IHex> CaptureEnclosedArea(ICharacter owner, IReadOnlyCollection<IHex> trail)
    {
        using var changes = BeginChanges();
        _conquestAlgorithm.ComputeCapturedArea(GetFixedByOwner(owner), trail, _hexGrid, _capturedBuffer);
        ApplyCapture(owner, _capturedBuffer);
        FillHoles(owner);
        return _capturedBuffer;
    }

    private void ApplyCapture(ICharacter owner, List<IHex> hexes)
    {
        CollectAffectedCharacters(owner, hexes);
        FixHexes(owner, hexes);

        foreach (var character in _affectedBuffer)
            ReleaseDisconnectedFragments(character);
    }

    private void CollectAffectedCharacters(ICharacter owner, List<IHex> hexes)
    {
        _affectedBuffer.Clear();

        foreach (var hex in hexes)
        {
            if (hex.Owner != null && hex.Owner != owner)
                _affectedBuffer.Add(hex.Owner);

            // Захват пустого гекса может разрезать территорию соседних персонажей
            foreach (var neighbor in _hexGrid.GetNeighbors(hex))
                if (neighbor.Owner != null && neighbor.Owner != owner)
                    _affectedBuffer.Add(neighbor.Owner);
        }
    }

    private void FillHoles(ICharacter owner)
    {
        // До сходимости: каждый новый захват может создать новые замкнутые области
        while (true)
        {
            _conquestAlgorithm.ComputeCapturedArea(GetFixedByOwner(owner), _emptyTrail, _hexGrid, _holesBuffer);

            if (_holesBuffer.Count == 0)
                break;

            ApplyCapture(owner, _holesBuffer);
        }
    }

    public void ReleaseDisconnectedFragments(ICharacter character)
    {
        using var changes = BeginChanges();
        var owned = _tracker.GetOwned(character);
        if (owned.Count <= 1)
            return;

        _fragmentVisited.Clear();
        _components.Clear();

        foreach (var startHex in owned)
        {
            if (_fragmentVisited.Contains(startHex))
                continue;

            _componentBuffer.Clear();
            _fragmentQueue.Clear();
            _fragmentQueue.Enqueue(startHex);
            _fragmentVisited.Add(startHex);
            _componentBuffer.Add(startHex);

            while (_fragmentQueue.Count > 0)
            {
                var current = _fragmentQueue.Dequeue();

                foreach (var neighbor in _hexGrid.GetNeighbors(current))
                {
                    if (_fragmentVisited.Contains(neighbor))
                        continue;

                    bool isOwned = _tracker.IsOwnedBy(character, neighbor);
                    bool isTrailBridge = neighbor.State == HexState.PartOfTrail;

                    if (!isOwned && !isTrailBridge)
                        continue;

                    _fragmentVisited.Add(neighbor);
                    _fragmentQueue.Enqueue(neighbor);

                    if (isOwned)
                        _componentBuffer.Add(neighbor);
                }
            }

            _components.Add(new List<IHex>(_componentBuffer));
        }

        if (_components.Count <= 1)
            return;

        int largestIndex = 0;
        for (int i = 1; i < _components.Count; i++)
            if (_components[i].Count > _components[largestIndex].Count)
                largestIndex = i;

        for (int i = 0; i < _components.Count; i++)
        {
            if (i == largestIndex)
                continue;

            foreach (var hex in _components[i])
                ReleaseHex(hex);
        }
    }
}
