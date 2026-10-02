using System;
using System.Collections.Generic;
using UnityEngine;

// Выдаёт ботам зависимости их поведения. Отделён от спавнера: появление персонажа и
// устройство AI меняются по разным причинам. Подписан на BotSpawned, а не на общее
// CharacterSpawned, поэтому dev-враг без мозга остаётся без мозга.
public class EnemyBrainBinder : MonoBehaviour
{
    [Required] [SerializeField] private BotPersonalitySettings _personality;

    private EnemySpawner _enemySpawner;
    private IHexGridProvider _grid;
    private IReadOnlyList<ICharacter> _allCharacters;
    private ICollectibleRegistry _collectibleRegistry;

    public void Init(EnemySpawner enemySpawner, IHexGridProvider grid,
        IReadOnlyList<ICharacter> allCharacters, ICollectibleRegistry collectibleRegistry)
    {
        _enemySpawner = enemySpawner ?? throw new ArgumentNullException(nameof(enemySpawner));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _allCharacters = allCharacters ?? throw new ArgumentNullException(nameof(allCharacters));
        _collectibleRegistry = collectibleRegistry ?? throw new ArgumentNullException(nameof(collectibleRegistry));

        _enemySpawner.BotSpawned += OnBotSpawned;
    }

    private void OnBotSpawned(Enemy enemy, int botIndex)
    {
        enemy.InitBrain(_grid, _allCharacters, _collectibleRegistry, _personality, botIndex);
    }

    private void OnDestroy()
    {
        if (_enemySpawner != null)
            _enemySpawner.BotSpawned -= OnBotSpawned;
    }
}
