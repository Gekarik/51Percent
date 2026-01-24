using System;
using System.Collections.Generic;
using System.Linq;

public class OwnershipTracker
{
    private readonly Dictionary<ICharacter, HashSet<IHex>> _byOwner = new Dictionary<ICharacter, HashSet<IHex>>();
    private readonly Dictionary<IHex, ICharacter> _ownersCache = new Dictionary<IHex, ICharacter>();

    public void Initialize(IEnumerable<IHex> allHexes)
    {
        _byOwner.Clear();
        _ownersCache.Clear();

        foreach (var h in allHexes)
        {
            var owner = h.Owner;
            _ownersCache[h] = owner;

            if (owner != null)
            {
                if (_byOwner.TryGetValue(owner, out var hexes) == false)
                {
                    hexes = new HashSet<IHex>();
                    _byOwner[owner] = hexes;
                }

                hexes.Add(h);
            }
        }
    }

    public void Subscribe(IEnumerable<IHex> allHexes)
    {
        foreach (var hex in allHexes)
            hex.StateChanged += OnHexStateChanged;
    }

    public void Unsubscribe(IEnumerable<IHex> allHexes)
    {
        foreach (var hex in allHexes)
            hex.StateChanged -= OnHexStateChanged;
    }

    private void OnHexStateChanged(IHex hex)
    {
        _ownersCache.TryGetValue(hex, out var prevOwner);
        var newOwner = hex.Owner;

        if (ReferenceEquals(prevOwner, newOwner))
            return;

        if (prevOwner != null && _byOwner.TryGetValue(prevOwner, out var prevSet))
        {
            prevSet.Remove(hex);

            if (prevSet.Count == 0)
                _byOwner.Remove(prevOwner);
        }

        if (newOwner != null)
        {
            if (!_byOwner.TryGetValue(newOwner, out var newSet))
            {
                newSet = new HashSet<IHex>();
                _byOwner[newOwner] = newSet;
            }

            newSet.Add(hex);
        }

        _ownersCache[hex] = newOwner;
    }

    public IReadOnlyCollection<IHex> GetOwned(ICharacter character)
    {
        if (character == null) 
            throw new ArgumentNullException(nameof(character));

        if (_byOwner.TryGetValue(character, out var hexes)) 
            return hexes;

        return Array.Empty<IHex>();
    }

    public void TakeOwnership(ICharacter character, IHex hex)
    {
        if (hex == null) 
            return;

        hex.SetOwner(character, HexState.Busy);
    }

    public void ReleaseAll(ICharacter character)
    {
        if (character == null)
            return;

        if (_byOwner.TryGetValue(character, out var hexes) == false)
            return;

        var owned = hexes.ToList();

        foreach (var h in owned)
            h.SetOwner(null, HexState.Empty);
    }

    public void Reset()
    {
        _byOwner.Clear();
        _ownersCache.Clear();
    }
}
