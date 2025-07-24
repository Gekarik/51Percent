public class KillManager 
{
    private Conquester _conquester;

    public KillManager(Conquester conquester)
    {
        _conquester = conquester;
        _conquester.TrailInterrupted += OnTrailInterrupted;
    }

    private void OnTrailInterrupted(ICharacter victim, ICharacter killer)
    {
        victim.Die();
        killer.Kill();
    }
}