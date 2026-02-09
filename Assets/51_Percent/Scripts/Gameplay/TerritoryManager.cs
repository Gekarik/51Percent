using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerritoryManager : MonoBehaviour
{
    [SerializeField] private HexGrid _hexGrid;

    private readonly OwnershipTracker _tracker = new OwnershipTracker();
    private TransformWaver _transformWaver;
    
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

    private void OnEnable()
    {
        _tracker.Subscribe(_hexGrid.AllHexes);
    }

    private void OnDisable()
    {
        _tracker.Unsubscribe(_hexGrid.AllHexes);
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

    public void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView)
    {
        _transformWaver?.Wave(hexesView);
    }

    private void Reset()
    {
        _tracker.Unsubscribe(_hexGrid.AllHexes);
        _tracker.Reset();
    }
}
