using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int enemyCount = 5;

    private void Start()
    {
        EnsureInitialized();
        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = SpawnSingleCharacter();
        }
    }
}
