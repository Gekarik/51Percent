using System;
using System.Collections.Generic;

public class WinConditionTracker
{
    private const float WinPercent = 0.51f;

    private readonly ITerritoryOwnership _territoryManager;
    private readonly IRespawnTracker _respawnTracker;
    private readonly MatchState _matchState;
    private readonly List<ICharacter> _aliveCharacters = new List<ICharacter>();
    private int _evaluationDepth;
    private bool _evaluationRequested;
    private bool _eliminationPending;

    public event Action<ICharacter> GameFinished;

    public WinConditionTracker(ITerritoryOwnership territoryManager, IRespawnTracker respawnTracker,
        MatchState matchState)
    {
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _respawnTracker = respawnTracker ?? throw new ArgumentNullException(nameof(respawnTracker));
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _territoryManager.OwnershipChanged += CheckTerritoryCondition;
    }

    public void RegisterCharacter(ICharacter character)
    {
        if (character != null && !_aliveCharacters.Contains(character))
            _aliveCharacters.Add(character);
    }

    public void OnCharacterEliminated(ICharacter character)
    {
        if (!_aliveCharacters.Remove(character))
            return;

        _eliminationPending = true;
        CheckTerritoryCondition();
    }

    public IDisposable DeferEvaluation()
    {
        _evaluationDepth++;
        return new EvaluationScope(this);
    }

    private bool HasAliveHuman()
    {
        foreach (var c in _aliveCharacters)
            if (c.IsHuman) return true;

        return false;
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

    public void ForceFinish(ICharacter winner) => FinishGame(winner);

    private void CheckTerritoryCondition()
    {
        _evaluationRequested = true;
        if (_evaluationDepth == 0)
            Evaluate();
    }

    private void Evaluate()
    {
        _evaluationRequested = false;
        if (_matchState.IsFinished)
            return;

        for (int i = 0; i < _aliveCharacters.Count; i++)
        {
            var character = _aliveCharacters[i];

            if (_territoryManager.GetOwnershipPercent(character) >= WinPercent)
            {
                FinishGame(character);
                return;
            }
        }

        if (!_eliminationPending || _respawnTracker.HasPendingRespawn)
            return;

        _eliminationPending = false;
        if (!HasAliveHuman())
            FinishGame(GetLeader());
        else if (_aliveCharacters.Count == 1)
            FinishGame(_aliveCharacters[0]);
    }

    private void FinishGame(ICharacter winner)
    {
        if (winner == null || winner.State != CharacterState.Alive || !_matchState.TryFinish())
            return;

        GameFinished?.Invoke(winner);
    }

    public void Dispose()
    {
        _territoryManager.OwnershipChanged -= CheckTerritoryCondition;
    }

    private sealed class EvaluationScope : IDisposable
    {
        private WinConditionTracker _owner;

        public EvaluationScope(WinConditionTracker owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (_owner == null)
                return;

            var owner = _owner;
            _owner = null;
            owner._evaluationDepth--;
            if (owner._evaluationDepth == 0 && owner._evaluationRequested)
                owner.Evaluate();
        }
    }
}
