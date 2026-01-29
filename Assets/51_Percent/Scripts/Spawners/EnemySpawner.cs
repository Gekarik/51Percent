using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int _enemyCount = 5;
    [SerializeField] private BotPersonalitySettings[] _personalities;

    private void Start()
    {
        EnsureInitialized();

        for (int i = 0; i < _enemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        var enemy = SpawnSingleCharacter();

        BotPersonalitySettings personality = GetRandomPersonality();
        enemy.InitBrain(_grid, personality);
    }

    private BotPersonalitySettings GetRandomPersonality()
    {
        if (_personalities == null || _personalities.Length == 0)
            return null;

        int randomIndex = Random.Range(0, _personalities.Length);
        return _personalities[randomIndex];
    }
}
