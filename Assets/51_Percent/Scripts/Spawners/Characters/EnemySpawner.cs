using System;
using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int _enemyCount = 5;
    [SerializeField] private BotNamesSO _botNames;
    [SerializeField] private Transform[] _spawnPoints;

    private int _spawnedCount;

    public event Action<Enemy, int> BotSpawned;

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
            result[i] = Grid.GetHexAt(_spawnPoints[i].position) ?? Grid.GetRandomHex();
        return result;
    }

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
        BotSpawned?.Invoke(enemy, personalityIndex);
    }
}
