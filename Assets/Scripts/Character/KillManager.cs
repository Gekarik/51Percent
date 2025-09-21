public class KillManager 
{
    public KillManager()
    {
        
    }

    public void OnTrailInterrupted(ICharacter victim, ICharacter killer)
    {
        victim.Die();
        killer.Kill();
    }
}