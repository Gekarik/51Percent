using System;

// Боевые правила атаки по трейлу. Исход зависит от активных эффектов участников, и эффекты
// спрашиваются напрямую: сами они ничего здесь не регистрируют, поэтому исход целиком
// определяется состоянием на момент атаки.
public class KillManager
{
    private readonly IMatchState _matchState;
    private readonly ICharacterElimination _elimination;

    public event Action<ICharacter> CharacterEliminated;

    public KillManager(IMatchState matchState, ICharacterElimination elimination)
    {
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _elimination = elimination ?? throw new ArgumentNullException(nameof(elimination));
    }

    public void OnTrailInterrupted(ICharacter trailOwner, ICharacter stepper)
    {
        if (!_matchState.IsRunning || trailOwner == null || stepper == null || trailOwner == stepper
            || trailOwner.State != CharacterState.Alive || stepper.State != CharacterState.Alive)
            return;

        // Этап 1: кто умирает (шипы владельца трейла меняют жертву и убийцу местами)
        var (victim, killer) = ResolveTrailKill(trailOwner, stepper);

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

    private (ICharacter victim, ICharacter killer) ResolveTrailKill(ICharacter trailOwner, ICharacter stepper)
    {
        return ActiveEffect(trailOwner) is ITrailKillModifier modifier
            ? modifier.ResolveTrailKill(trailOwner, stepper)
            : (trailOwner, stepper);
    }

    private bool TryEscapeDeath(ICharacter victim)
    {
        return ActiveEffect(victim) is IDeathInterceptor interceptor && interceptor.TryEscapeDeath();
    }

    private IBoosterEffect ActiveEffect(ICharacter character)
    {
        return character?.BoosterObservable?.ActiveEffect;
    }
}
