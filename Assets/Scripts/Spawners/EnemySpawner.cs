using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DEPRECATED: Старый спавнер врагов
/// Используйте IoGameBotSpawner для новой системы ботов
/// </summary>
[System.Obsolete("Use IoGameBotSpawner instead for better AI and performance")]
public class EnemySpawner : CharacterSpawner<Enemy>
{
    [SerializeField] private int enemyCount = 5;
    
    [Header("Migration Notice")]
    [SerializeField, TextArea(3, 5)] 
    private string migrationNotice = "⚠️ DEPRECATED: Replace with IoGameBotSpawner\n" +
                                   "New system provides:\n" +
                                   "• Better AI with curved paths\n" +
                                   "• Configurable behavior\n" +
                                   "• Performance optimizations\n" +
                                   "• Multiple bot personalities";

    private void Start()
    {
        Debug.LogWarning($"EnemySpawner is deprecated! Use IoGameBotSpawner instead. GameObject: {gameObject.name}");
        
        // Старая логика (сохранена для совместимости)
        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = SpawnSingleCharacter();
            enemy.InitAI(_grid);
        }   
    }
    
    /// <summary>
    /// Автоматическая миграция на новую систему (только для разработки)
    /// </summary>
    [ContextMenu("Migrate to IoGameBotSpawner")]
    private void MigrateToNewSystem()
    {
        Debug.Log("To migrate to IoGameBotSpawner:");
        Debug.Log("1. Add IoGameBotSpawner component");
        Debug.Log("2. Set botCount = " + enemyCount);
        Debug.Log("3. Configure bot personalities");
        Debug.Log("4. Remove this EnemySpawner component");
        Debug.Log("5. Update prefab references from Enemy to IoGameBot");
    }
}
