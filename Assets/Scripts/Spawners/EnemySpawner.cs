using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;  // теперь префаб Enemy, а не AIController
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private HexGrid hexGrid;    // drag & drop

    private readonly List<Enemy> _enemies = new();

    private void Start()
    {
        var allHexes = hexGrid.AllHexes;
        for (int i = 0; i < enemyCount; i++)
        {
            var spawnHex = allHexes[Random.Range(0, allHexes.Count)];
            var e = Instantiate(enemyPrefab, spawnHex.transform.position, Quaternion.identity);
            e.Init(hexGrid, spawnHex);
            _enemies.Add(e);
        }
    }
}