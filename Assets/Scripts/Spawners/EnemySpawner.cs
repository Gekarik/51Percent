using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int enemyCount = 5;

    private void Start()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = SpawnSingleCharacter();
            enemy.InitAI(_grid);
        }   
    }
}
