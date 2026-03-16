using System;
using System.Collections.Generic;

public class WinConditionTracker
{
    private const float WinPercent = 0.51f;

    private readonly TerritoryManager _territoryManager;
    private readonly List<ICharacter> _aliveCharacters = new List<ICharacter>();
    private bool _isGameOver;

    public event Action<ICharacter> GameFinished;

    public WinConditionTracker(TerritoryManager territoryManager)
    {
        _territoryManager = territoryManager;
        _territoryManager.OwnershipChanged += CheckTerritoryCondition;
    }

    public void RegisterCharacter(ICharacter character)
    {
        _aliveCharacters.Add(character);
    }

    public void OnCharacterEliminated(ICharacter character)
    {
        _aliveCharacters.Remove(character);

        if (character is Player)
            FinishGame(GetLeader());
    }

    private ICharacter GetLeader()
    {
        if (_aliveCharacters.Count == 0)
            return null;

        ICharacter leader = _aliveCharacters[0];
        float maxPercent = _territoryManager.GetOwnershipPercent(leader);

        for (int i = 1; i < _aliveCharacters.Count; i++)
        {
            float percent = _territoryManager.GetOwnershipPercent(_aliveCharacters[i]);
            if (percent > maxPercent)
            {
                maxPercent = percent;
                leader = _aliveCharacters[i];
            }
        }

        return leader;
    }

    private void CheckTerritoryCondition()
    {
        for (int i = 0; i < _aliveCharacters.Count; i++)
        {
            var character = _aliveCharacters[i];

            if (_territoryManager.GetOwnershipPercent(character) >= WinPercent)
            {
                FinishGame(character);
                return;
            }
        }
    }

    private void FinishGame(ICharacter winner)
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        GameFinished?.Invoke(winner);
    }

    public void Dispose()
    {
        _territoryManager.OwnershipChanged -= CheckTerritoryCondition;
    }
}
