using System;
using UnityEngine;

[CreateAssetMenu(menuName = "51_Percent/Spawn Points", fileName = "SpawnPoints")]
public class SpawnPointsSO : ScriptableObject
{
    [Tooltip("Точка спавна игрока (Q, R в координатах гекс-сетки)")]
    [SerializeField] private Vector2Int _playerSpawnPoint;

    [Tooltip("Точки спавна ботов по порядку (Q, R). Если ботов больше чем точек — остаток спавнится случайно")]
    [SerializeField] private Vector2Int[] _enemySpawnPoints;

    public IHex[] ResolvePlayerHexes(HexGrid grid)
    {
        IHex hex = grid.GetHex(new HexCoord(_playerSpawnPoint.x, _playerSpawnPoint.y));
        return new[] { hex ?? grid.GetRandomHex() };
    }

    public IHex[] ResolveEnemyHexes(HexGrid grid)
    {
        if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            return Array.Empty<IHex>();

        var result = new IHex[_enemySpawnPoints.Length];
        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            var p = _enemySpawnPoints[i];
            result[i] = grid.GetHex(new HexCoord(p.x, p.y)) ?? grid.GetRandomHex();
        }

        return result;
    }
}
