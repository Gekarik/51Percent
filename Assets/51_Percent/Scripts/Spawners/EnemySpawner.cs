using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int _enemyCount = 5;
    [Required] [SerializeField] private BotPersonalitySettings _personality;
    [SerializeField] private BotNamesSO _botNames;
    [SerializeField] private Transform[] _spawnPoints;

    private IReadOnlyList<ICharacter> _allCharacters;
    private ICollectibleRegistry _collectibleRegistry;
    private int _spawnedCount;

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

    // Dev-инструмент: враг без мозга — стоит на месте, но полноценно участвует в правилах игры
    public ICharacter SpawnIdleEnemy()
    {
        var enemy = SpawnNext();
        enemy.SetName("AFK");
        RegisterInLeaderBoard(enemy);
        return enemy;
    }

    private void SpawnEnemy()
    {
        _spawnedCount++;
        int personalityIndex = _spawnedCount - 1;
        string name = _botNames != null ? _botNames.GetNext() : $"Bot {_spawnedCount}";

        var enemy = SpawnNext();
        enemy.SetName(name);
        RegisterInLeaderBoard(enemy);
        enemy.InitBrain(_grid, _allCharacters, _collectibleRegistry, _personality, personalityIndex);
    }
}
