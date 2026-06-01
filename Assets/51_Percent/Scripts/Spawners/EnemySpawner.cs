using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int _enemyCount = 5;
    [SerializeField] private BotPersonalitySettings _personality;
    [SerializeField] private BotNamesSO _botNames;
    [SerializeField] private Transform[] _spawnPoints;

    private IReadOnlyList<ICharacter> _allCharacters;
    private ICollectibleRegistry _collectibleRegistry;
    private int _spawnedCount;
    private readonly Dictionary<Enemy, int> _personalityIndices = new Dictionary<Enemy, int>();

    public void SetAIReferences(IReadOnlyList<ICharacter> allCharacters, ICollectibleRegistry collectibleRegistry)
    {
        _allCharacters = allCharacters;
        _collectibleRegistry = collectibleRegistry;
    }

    private void Start()
    {
        EnsureInitialized();
        SetSpawnHexes(ResolveSpawnHexes());

        for (int i = 0; i < _enemyCount; i++)
            SpawnEnemy();
    }

    private IHex[] ResolveSpawnHexes()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
            return Array.Empty<IHex>();

        var result = new IHex[_spawnPoints.Length];
        for (int i = 0; i < _spawnPoints.Length; i++)
            result[i] = _grid.GetHexAt(_spawnPoints[i].position) ?? _grid.GetRandomHex();
        return result;
    }

    private void SpawnEnemy()
    {
        _spawnedCount++;
        int personalityIndex = _spawnedCount - 1;
        string name = _botNames != null ? _botNames.GetNext() : $"Bot {_spawnedCount}";

        var enemy = SpawnNext();
        enemy.SetName(name);
        _personalityIndices[enemy] = personalityIndex;
        RegisterInLeaderBoard(enemy);
        enemy.InitBrain(_grid, _allCharacters, _collectibleRegistry, _personality, personalityIndex);
    }

    protected override void OnRespawnRequested(Enemy character)
    {
        int personalityIndex = _personalityIndices.TryGetValue(character, out var idx) ? idx : 0;

        var newEnemy = SpawnAt(_grid.GetRandomHex());
        newEnemy.SetName(character.Name);
        _personalityIndices[newEnemy] = personalityIndex;
        RegisterInLeaderBoard(newEnemy);
        newEnemy.InitBrain(_grid, _allCharacters, _collectibleRegistry, _personality, personalityIndex);
    }
}
