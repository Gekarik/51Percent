using System;
using System.Collections.Generic;

public class KillManager
{
    private readonly Dictionary<ICharacter, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)>> _resolvers
        = new Dictionary<ICharacter, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)>>();

    public event Action<ICharacter> CharacterEliminated;

    public void RegisterResolver(ICharacter character, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)> resolver)
    {
        _resolvers[character] = resolver;
    }

    public void UnregisterResolver(ICharacter character)
    {
        _resolvers.Remove(character);
    }

    public void OnTrailInterrupted(ICharacter trailOwner, ICharacter stepper)
    {
        var (victim, killer) = _resolvers.TryGetValue(trailOwner, out var resolver)
            ? resolver(trailOwner, stepper)
            : (trailOwner, stepper);

        victim.Die();
        killer.Kill();
        CharacterEliminated?.Invoke(victim);
    }

    public void OnTrailOrphaned(ICharacter victim)
    {
        victim.Die();
        CharacterEliminated?.Invoke(victim);
    }
}
