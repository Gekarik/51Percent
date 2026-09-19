using System;
using System.Collections.Generic;

public class KillManager
{
    private readonly IMatchState _matchState;
    private readonly ICharacterElimination _elimination;
    private readonly Dictionary<ICharacter, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)>> _resolvers
        = new Dictionary<ICharacter, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)>>();

    // Перехватчик получает шанс отменить смерть (например, крылья уносят жертву домой)
    private readonly Dictionary<ICharacter, Func<bool>> _deathInterceptors = new Dictionary<ICharacter, Func<bool>>();

    public event Action<ICharacter> CharacterEliminated;

    public KillManager(IMatchState matchState, ICharacterElimination elimination)
    {
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _elimination = elimination ?? throw new ArgumentNullException(nameof(elimination));
    }

    public void RegisterResolver(ICharacter character, Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)> resolver)
    {
        _resolvers[character] = resolver;
    }

    public void UnregisterResolver(ICharacter character)
    {
        _resolvers.Remove(character);
    }

    public void RegisterDeathInterceptor(ICharacter character, Func<bool> interceptor)
    {
        _deathInterceptors[character] = interceptor;
    }

    public void UnregisterDeathInterceptor(ICharacter character)
    {
        _deathInterceptors.Remove(character);
    }

    public void OnTrailInterrupted(ICharacter trailOwner, ICharacter stepper)
    {
        if (!_matchState.IsRunning || trailOwner == null || stepper == null || trailOwner == stepper
            || trailOwner.State != CharacterState.Alive || stepper.State != CharacterState.Alive)
            return;

        // Этап 1: кто умирает (шипы меняют жертву и убийцу местами)
        var (victim, killer) = _resolvers.TryGetValue(trailOwner, out var resolver)
            ? resolver(trailOwner, stepper)
            : (trailOwner, stepper);

        // Этап 2: состоится ли смерть. Спасение — атака сорвалась, килл не засчитывается
        if (TryEscapeDeath(victim))
            return;

        if (_elimination.TryEliminate(victim, killer))
            CharacterEliminated?.Invoke(victim);
    }

    public void OnTrailOrphaned(ICharacter victim)
    {
        if (!_matchState.IsRunning || victim == null || victim.State != CharacterState.Alive)
            return;

        if (TryEscapeDeath(victim))
            return;

        if (_elimination.TryEliminate(victim, null))
            CharacterEliminated?.Invoke(victim);
    }

    private bool TryEscapeDeath(ICharacter victim)
    {
        return _deathInterceptors.TryGetValue(victim, out var interceptor) && interceptor();
    }
}
