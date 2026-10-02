using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSpawner<T> : MonoBehaviour where T : CharacterBase
{
    [Required] [SerializeField] private T _prefab;

    protected KillManager _killManager;
    protected LeaderBoardModel _leaderBoardModel;
    protected WinConditionTracker _winConditionTracker;

    private CharacterDependencies _dependencies;

    // Спавнеру грид нужен для выбора места; остальные зависимости персонажа
    // он не читает и только отдаёт фабрике
    protected IHexGridProvider Grid => _dependencies.Grid;

    private IHex[] _spawnHexes;
    private int _spawnIndex;
    private bool _initialized;
    private List<ICharacter> _allCharacters;
    private SpawnHexSelector _spawnHexSelector;

    protected CharacterFactory<T> _factory;

    public event Action<ICharacter> CharacterSpawned;

    public void Init(CharacterDependencies dependencies, KillManager killManager,
        LeaderBoardModel leaderBoardModel, WinConditionTracker winConditionTracker)
    {
        _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        _killManager = killManager ?? throw new ArgumentNullException(nameof(killManager));
        _leaderBoardModel = leaderBoardModel ?? throw new ArgumentNullException(nameof(leaderBoardModel));
        _winConditionTracker = winConditionTracker ?? throw new ArgumentNullException(nameof(winConditionTracker));

        _factory = new CharacterFactory<T>(_prefab, _dependencies);
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
        character.Trail.TrailInterrupted += _killManager.OnTrailInterrupted;
        character.Trail.TrailOrphaned += _killManager.OnTrailOrphaned;
        _winConditionTracker.RegisterCharacter(character);
        _allCharacters?.Add(character);
        CharacterSpawned?.Invoke(character);
        return character;
    }

    protected IHex GetNextHex()
    {
        if (_spawnHexes != null && _spawnIndex < _spawnHexes.Length)
            return _spawnHexes[_spawnIndex++];

        // Селектор создаётся здесь, а не в Init: список участников наполняется спавнами
        // и должен читаться на момент выбора, а не на момент настройки спавнера
        _spawnHexSelector ??= new SpawnHexSelector(Grid, _allCharacters);
        return _spawnHexSelector.SelectSpawnHex();
    }

    protected void RegisterInLeaderBoard(ICharacter character)
    {
        _leaderBoardModel.RegisterCharacter(character);
    }
}
