using System;
using System.Collections.Generic;
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

    private IHex[] _spawnHexes;
    private int _spawnIndex;
    private bool _initialized;
    private List<ICharacter> _allCharacters;

    protected CharacterFactory<T> _factory;

    public void Init(HexGrid grid, TerritoryManager territoryManager, KillManager killManager,
        ColorService colorService, LeaderBoardModel leaderBoardModel, WinConditionTracker winConditionTracker)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _territoryManager = territoryManager ?? throw new ArgumentNullException(nameof(territoryManager));
        _killManager = killManager ?? throw new ArgumentNullException(nameof(killManager));
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        _leaderBoardModel = leaderBoardModel ?? throw new ArgumentNullException(nameof(leaderBoardModel));
        _winConditionTracker = winConditionTracker ?? throw new ArgumentNullException(nameof(winConditionTracker));

        _factory = new CharacterFactory<T>(_prefab, _colorService, _territoryManager, _grid, _killManager);
        _initialized = true;
    }

    public void SetCharacterList(List<ICharacter> allCharacters)
    {
        _allCharacters = allCharacters;
    }

    public void SetSpawnHexes(IHex[] hexes)
    {
        _spawnHexes = hexes;
        _spawnIndex = 0;
    }

    protected void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException($"{GetType().Name} was not initialized. Call Init() first.");
    }

    protected T SpawnNext()
    {
        return SpawnAt(GetNextHex());
    }

    protected T SpawnAt(IHex hex)
    {
        EnsureInitialized();
        var character = _factory.Create(hex);
        character.TrailInterrupted += _killManager.OnTrailInterrupted;
        character.TrailOrphaned += _killManager.OnTrailOrphaned;
        character.RespawnRequested += () => OnRespawnRequested(character);
        _winConditionTracker.RegisterCharacter(character);
        _allCharacters?.Add(character);
        return character;
    }

    protected virtual void OnRespawnRequested(T character) { }

    protected IHex GetNextHex()
    {
        if (_spawnHexes != null && _spawnIndex < _spawnHexes.Length)
            return _spawnHexes[_spawnIndex++];

        return _grid.GetRandomHex();
    }

    protected void RegisterInLeaderBoard(ICharacter character)
    {
        _leaderBoardModel.RegisterCharacter(character);
    }
}
