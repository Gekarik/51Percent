using System;
using System.Collections.Generic;

public class OwnershipTracker
{
    private readonly Dictionary<ICharacter, HashSet<IHex>> _byOwner = new Dictionary<ICharacter, HashSet<IHex>>();

    public event Action OwnershipChanged;

    public void Initialize(IEnumerable<IHex> allHexes)
    {
        if (allHexes == null)
            return;

        _byOwner.Clear();

        foreach (var h in allHexes)
        {
            if (h == null) continue;
            var owner = h.Owner;

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

    private readonly HashSet<IHex> _emptySet = new HashSet<IHex>();

    public HashSet<IHex> GetOwned(ICharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        return _byOwner.TryGetValue(character, out var hexes) ? hexes : _emptySet;
    }

    public void TakeOwnership(ICharacter character, IHex hex)
    {
        if (hex == null)
            return;

        var prevOwner = hex.Owner;

        if (prevOwner != null && _byOwner.TryGetValue(prevOwner, out var prevSet))
        {
            prevSet.Remove(hex);

            if (prevSet.Count == 0)
                _byOwner.Remove(prevOwner);
        }

        if (character != null)
        {
            if (!_byOwner.TryGetValue(character, out var newSet))
            {
                newSet = new HashSet<IHex>();
                _byOwner[character] = newSet;
            }

            newSet.Add(hex);
        }

        hex.SetOwner(character, HexState.Busy);
        OwnershipChanged?.Invoke();
    }

    public void TransferToTrail(ICharacter newOwner, IHex hex)
    {
        if (hex == null)
            return;

        var prevOwner = hex.Owner;
        if (prevOwner != null && _byOwner.TryGetValue(prevOwner, out var prevSet))
        {
            prevSet.Remove(hex);
            if (prevSet.Count == 0)
                _byOwner.Remove(prevOwner);
        }

        hex.SetOwner(newOwner, HexState.PartOfTrail);
    }

    public void ReleaseHex(IHex hex)
    {
        if (hex == null)
            return;

        var owner = hex.Owner;

        if (owner != null && _byOwner.TryGetValue(owner, out var set))
        {
            set.Remove(hex);
            if (set.Count == 0)
                _byOwner.Remove(owner);
        }

        hex.SetOwner(null, HexState.Empty);
        OwnershipChanged?.Invoke();
    }

    private readonly List<IHex> _releaseBuffer = new List<IHex>();

    public void ReleaseAll(ICharacter character)
    {
        if (character == null)
            return;

        if (_byOwner.TryGetValue(character, out var hexes) == false)
            return;

        _releaseBuffer.Clear();
        foreach (var h in hexes)
            _releaseBuffer.Add(h);

        _byOwner.Remove(character);

        foreach (var h in _releaseBuffer)
            h.SetOwner(null, HexState.Empty);
    }

    public void Reset()
    {
        _byOwner.Clear();
    }
}
