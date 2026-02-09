using System;

public class KillManager
{
    public event Action<ICharacter> CharacterEliminated;

    public void OnTrailInterrupted(ICharacter victim, ICharacter killer)
    {
        victim.Die();
        killer.Kill();
        CharacterEliminated?.Invoke(victim);
    }
}
