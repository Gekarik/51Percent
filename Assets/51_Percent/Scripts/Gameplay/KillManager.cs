public class KillManager
{
    private readonly LeaderBoardModel _leaderBoardModel;

    public KillManager(LeaderBoardModel leaderBoardModel)
    {
        _leaderBoardModel = leaderBoardModel;
    }

    public void OnTrailInterrupted(ICharacter victim, ICharacter killer)
    {
        _leaderBoardModel?.UnregisterCharacter(victim);
        victim.Die();
        killer.Kill();
    }
}