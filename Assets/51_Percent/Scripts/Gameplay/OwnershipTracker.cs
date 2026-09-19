using System;
using System.Collections.Generic;

public class OwnershipTracker : IDisposable
{
    // Источник правды о владельце — сам Hex; здесь только производный индекс character → hexes.
    // Индекс обновляется по событию StateChanged, поэтому не может разойтись с гексами.
    private readonly Dictionary<ICharacter, HashSet<IHex>> _byOwner = new Dictionary<ICharacter, HashSet<IHex>>();
    private readonly Dictionary<IHex, ICharacter> _indexedOwnerByHex = new Dictionary<IHex, ICharacter>();
    private readonly List<IHex> _trackedHexes = new List<IHex>();
    private readonly List<IHex> _releaseBuffer = new List<IHex>();

    private readonly IReadOnlyCollection<IHex> _empty = Array.Empty<IHex>();
    private int _changeDepth;
    private bool _ownershipChanged;

    public event Action OwnershipChanged;

    // Индекс всегда актуален; внешние наблюдатели получают только итог всей операции.
    public IDisposable BeginChanges()
    {
        _changeDepth++;
        return new OwnershipChangeScope(this);
    }

    public void Initialize(IEnumerable<IHex> allHexes)
    {
        if (allHexes == null)
            return;

        Dispose();

        foreach (var hex in allHexes)
        {
            if (hex == null)
                continue;

            hex.StateChanged += OnHexStateChanged;
            _trackedHexes.Add(hex);
            IndexHex(hex);
        }
    }

    public void Dispose()
    {
        foreach (var hex in _trackedHexes)
            hex.StateChanged -= OnHexStateChanged;

        _trackedHexes.Clear();
        _byOwner.Clear();
        _indexedOwnerByHex.Clear();
    }

    public IReadOnlyCollection<IHex> GetOwned(ICharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        return _byOwner.TryGetValue(character, out var hexes) ? hexes : _empty;
    }

    public bool IsOwnedBy(ICharacter character, IHex hex)
    {
        return _indexedOwnerByHex.TryGetValue(hex, out var owner) && owner == character;
    }

    public void TakeOwnership(ICharacter character, IHex hex)
    {
        hex?.SetOwner(character, HexState.Busy);
    }

    public void TransferToTrail(ICharacter newOwner, IHex hex)
    {
        hex?.SetOwner(newOwner, HexState.PartOfTrail);
    }

    public void ReleaseHex(IHex hex)
    {
        hex?.SetOwner(null, HexState.Empty);
    }

    public void ReleaseAll(ICharacter character)
    {
        using var changes = BeginChanges();
        if (character == null || _byOwner.TryGetValue(character, out var hexes) == false)
            return;

        // Копия: SetOwner через событие меняет тот же самый набор
        _releaseBuffer.Clear();
        _releaseBuffer.AddRange(hexes);

        foreach (var hex in _releaseBuffer)
            hex.SetOwner(null, HexState.Empty);
    }

    private void OnHexStateChanged(IHex hex)
    {
        if (IndexHex(hex))
        {
            _ownershipChanged = true;
            PublishChanges();
        }
    }

    private void PublishChanges()
    {
        if (_changeDepth != 0 || !_ownershipChanged)
            return;

        _ownershipChanged = false;
        OwnershipChanged?.Invoke();
    }

    // Приводит индекс в соответствие с текущим состоянием гекса.
    // Закреплённым считается только Busy-гекс с владельцем.
    private bool IndexHex(IHex hex)
    {
        bool isFixed = hex.Owner != null && hex.State == HexState.Busy;

        if (_indexedOwnerByHex.TryGetValue(hex, out var indexedOwner))
        {
            if (isFixed && indexedOwner == hex.Owner)
                return false;

            RemoveFromIndex(hex, indexedOwner);

            if (isFixed)
                AddToIndex(hex);

            return true;
        }

        if (isFixed == false)
            return false;

        AddToIndex(hex);
        return true;
    }

    private void AddToIndex(IHex hex)
    {
        if (_byOwner.TryGetValue(hex.Owner, out var hexes) == false)
        {
            hexes = new HashSet<IHex>();
            _byOwner[hex.Owner] = hexes;
        }

        hexes.Add(hex);
        _indexedOwnerByHex[hex] = hex.Owner;
    }

    private void RemoveFromIndex(IHex hex, ICharacter indexedOwner)
    {
        _indexedOwnerByHex.Remove(hex);

        if (_byOwner.TryGetValue(indexedOwner, out var hexes) == false)
            return;

        hexes.Remove(hex);

        if (hexes.Count == 0)
            _byOwner.Remove(indexedOwner);
    }

    private sealed class OwnershipChangeScope : IDisposable
    {
        private OwnershipTracker _owner;

        public OwnershipChangeScope(OwnershipTracker owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (_owner == null)
                return;

            var owner = _owner;
            _owner = null;
            owner._changeDepth--;
            owner.PublishChanges();
        }
    }
}
