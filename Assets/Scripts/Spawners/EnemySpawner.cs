using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected Enemy enemyPrefab;//берём из скинов, скины локаем при взятии
    [SerializeField] private int enemyCount = 5;
    [SerializeField] protected HexGrid _hexGrid;

    private void Start()
    {
        var allHexes = _hexGrid.AllHexes;
        for (int i = 0; i < enemyCount; i++)
        {
            var spawnHex = allHexes[Random.Range(0, allHexes.Count)];
            var e = Instantiate(enemyPrefab, spawnHex.transform.position, Quaternion.identity);
            e.InitConquester(_hexGrid, spawnHex);
            e.InitAI(_hexGrid);
        }
    }
}
