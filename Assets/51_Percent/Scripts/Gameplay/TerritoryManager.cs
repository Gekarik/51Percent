using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerritoryManager : MonoBehaviour
{
    [SerializeField] private HexGrid _hexGrid;

    private readonly OwnershipTracker _tracker = new OwnershipTracker();
    private TransformWaver _transformWaver;

    private readonly HashSet<IHex> _fragmentVisited = new HashSet<IHex>();
    private readonly Queue<IHex> _fragmentQueue = new Queue<IHex>();
    private readonly List<List<IHex>> _components = new List<List<IHex>>();
    private readonly List<IHex> _componentBuffer = new List<IHex>();
    
    public int AllHexes => _hexGrid.AllHexes.Count;

    public event Action OwnershipChanged
    {
        add => _tracker.OwnershipChanged += value;
        remove => _tracker.OwnershipChanged -= value;
    }

    private void Awake()
    {
        if (_hexGrid == null)
            throw new NullReferenceException(nameof(_hexGrid));

        _transformWaver = new TransformWaver();
        _tracker.Initialize(_hexGrid.AllHexes);
    }

    public void InitCharacter(ICharacter character)
    {
        if (character == null) 
            throw new ArgumentNullException(nameof(character));
    }

    public void GetStartTerritory(ICharacter character, IHex startHex)
    {
        IEnumerable<IHex> hexes = _hexGrid.GetNeighbors(startHex).Append(startHex);
        FixHexes(character, hexes);
    }

    public IReadOnlyCollection<IHex> GetFixedByOwner(ICharacter character)
    {
        return _tracker.GetOwned(character);
    }

    public float GetOwnershipPercent(ICharacter character)
    {
        int totalHexes = _hexGrid.AllHexes.Count;
        if (totalHexes == 0)
            return 0f;

        return (float)_tracker.GetOwned(character).Count / totalHexes;
    }

    public void OnCharacterDied(ICharacter character)
    {
        _tracker.ReleaseAll(character);
    }

    public void FixHexes(ICharacter character, IEnumerable<IHex> hexes)
    {
        foreach (var h in hexes)
            FixHex(character, h);
    }

    public void FixHex(ICharacter character, IHex hex)
    {
        _tracker.TakeOwnership(character, hex);
    }

    public void ReleaseHex(IHex hex)
    {
        _tracker.ReleaseHex(hex);
    }

    public void TrailHex(ICharacter character, IHex hex)
    {
        _tracker.TransferToTrail(character, hex);
    }

    public void ReleaseDisconnectedFragments(ICharacter character)
    {
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

                    bool isOwned = owned.Contains(neighbor);
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
                _tracker.ReleaseHex(hex);
        }
    }

    public void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView)
    {
        _transformWaver?.Wave(hexesView);
    }

    private void Reset()
    {
        _tracker.Reset();
    }
}
