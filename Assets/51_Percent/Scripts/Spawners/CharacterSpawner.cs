using System;
using UnityEngine;

public abstract class CharacterSpawner<T> : MonoBehaviour where T : MonoBehaviour, ICharacter
{
    [SerializeField] private T _prefab;

    protected HexGrid _grid;
    protected TerritoryManager _territoryManager;
    protected KillManager _killManager;
    protected ColorService _colorService;
    protected LeaderBoardModel _leaderBoardModel;
    protected WinConditionTracker _winConditionTracker;

    private bool _initialized;

    public void Init(HexGrid grid, TerritoryManager territoryManager, KillManager killManager,
        ColorService colorService, LeaderBoardModel leaderBoardModel, WinConditionTracker winConditionTracker)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _killManager = killManager ?? throw new ArgumentNullException(nameof(killManager));
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        _leaderBoardModel = leaderBoardModel ?? throw new ArgumentNullException(nameof(leaderBoardModel));
        _winConditionTracker = winConditionTracker ?? throw new ArgumentNullException(nameof(winConditionTracker));
        _initialized = true;
    }

    protected void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException($"{GetType().Name} was not initialized. Call Init() first.");
    }

    protected T SpawnSingleCharacter()
    {
        EnsureInitialized();

        IHex startHex = _grid.GetRandomHex();

        var character = Instantiate(_prefab, startHex.Transform.position, Quaternion.identity);
        character.Init(_colorService, _territoryManager, _grid);
        _territoryManager.GetStartTerritory(character, startHex);
        character.Conqueror.TrailInterrupted += _killManager.OnTrailInterrupted;
        _winConditionTracker.RegisterCharacter(character);

        return character;
    }

    protected void RegisterInLeaderBoard(ICharacter character)
    {
        _leaderBoardModel.RegisterCharacter(character);
    }
}
