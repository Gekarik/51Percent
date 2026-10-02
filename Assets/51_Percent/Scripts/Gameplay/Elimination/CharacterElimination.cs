using System;

public class CharacterElimination : ICharacterElimination
{
    private readonly ITerritoryChanges _territory;
    private readonly IRespawnScheduler _respawner;
    private readonly WinConditionTracker _winConditions;
    private readonly LeaderBoardModel _leaderBoard;
    private readonly ICoinScatterer _coins;

    public CharacterElimination(ITerritoryChanges territory, IRespawnScheduler respawner,
        WinConditionTracker winConditions, LeaderBoardModel leaderBoard, ICoinScatterer coins)
    {
        _territory = territory ?? throw new ArgumentNullException(nameof(territory));
        _respawner = respawner ?? throw new ArgumentNullException(nameof(respawner));
        _winConditions = winConditions ?? throw new ArgumentNullException(nameof(winConditions));
        _leaderBoard = leaderBoard ?? throw new ArgumentNullException(nameof(leaderBoard));
        _coins = coins ?? throw new ArgumentNullException(nameof(coins));
    }

    public bool TryEliminate(ICharacter victim, ICharacter killer)
    {
        if (victim == null || victim.State != CharacterState.Alive)
            return false;

        using (_winConditions.DeferEvaluation())
        using (_territory.BeginChanges())
        {
            if (!victim.TryDie())
                return false;

            killer?.Kill();
            _respawner.TryScheduleRespawn(victim);
            _winConditions.OnCharacterEliminated(victim);

            if (victim.LifeStats.Coins > 0)
                _coins.ScatterCoins(victim.Transform.position, victim.LifeStats.Coins);

            _leaderBoard.UnregisterCharacter(victim);
        }

        return true;
    }
}
