using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Спавнер IoGameBot с различными конфигурациями
/// </summary>
public class IoGameBotSpawner : CharacterSpawner<IoGameBot>
{
    [Header("Spawn Settings")]
    [SerializeField] private int botCount = 5;
    
    [Header("Bot Configurations")]
    [SerializeField] private List<IoGameBotConfig> botConfigs = new List<IoGameBotConfig>();
    [SerializeField] private bool useRandomConfigs = true;
    
    [Header("Default Config")]
    [SerializeField] private IoGameBotConfig defaultConfig = new IoGameBotConfig();
    
    private void Start()
    {
        SpawnBots();
    }
    
    /// <summary>
    /// Спавнит ботов с различными конфигурациями
    /// </summary>
    private void SpawnBots()
    {
        for (int i = 0; i < botCount; i++)
        {
            var bot = SpawnSingleCharacter();
            
            // Применяем конфигурацию
            ApplyBotConfiguration(bot, i);
            
            // Инициализируем бота
            bot.Init(_grid);
            
            Debug.Log($"Spawned IoGameBot #{i + 1} with config: aggressiveness={bot.GetType().GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(bot) ?? "default"}");
        }
    }
    
    /// <summary>
    /// Применяет конфигурацию к боту
    /// </summary>
    private void ApplyBotConfiguration(IoGameBot bot, int botIndex)
    {
        IoGameBotConfig configToApply;
        
        if (useRandomConfigs)
        {
            // Создаем случайную конфигурацию
            configToApply = CreateRandomConfig();
        }
        else if (botConfigs.Count > 0)
        {
            // Используем предустановленные конфигурации циклично
            configToApply = botConfigs[botIndex % botConfigs.Count];
        }
        else
        {
            // Используем дефолтную конфигурацию
            configToApply = defaultConfig;
        }
        
        // Применяем конфигурацию через рефлексию (поскольку поле приватное)
        var configField = typeof(IoGameBot).GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configField != null)
        {
            configField.SetValue(bot, configToApply);
        }
        
        Debug.Log($"Applied config to bot {bot.name}: aggressiveness={configToApply.aggressiveness:F1}, curvature={configToApply.pathCurvature:F1}");
    }
    
    /// <summary>
    /// Создает случайную конфигурацию для разнообразия поведения
    /// </summary>
    private IoGameBotConfig CreateRandomConfig()
    {
        var config = new IoGameBotConfig();
        
        // Случайная агрессивность (осторожные и смелые боты)
        config.aggressiveness = Random.Range(3f, 15f);
        
        // Случайная кривизна пути (прямолинейные и извилистые)
        config.pathCurvature = Random.Range(0.8f, 4f);
        
        // Случайный радиус сбора
        config.collectRadius = Random.Range(2f, 6f);
        
        // Случайная агрессия к трейлам
        config.trailHuntingChance = Random.Range(0.1f, 0.6f);
        
        // Случайная скорость решений
        config.decisionSpeed = Random.Range(0.7f, 2f);
        
        // Случайность в поведении
        config.randomness = Random.Range(0.1f, 0.4f);
        
        return config;
    }
    
    /// <summary>
    /// Создает предустановленные конфигурации для тестирования
    /// </summary>
    [ContextMenu("Create Preset Configs")]
    private void CreatePresetConfigs()
    {
        botConfigs.Clear();
        
        // Осторожный бот
        var cautiousBot = new IoGameBotConfig
        {
            aggressiveness = 4f,
            pathCurvature = 1.5f,
            collectRadius = 4f,
            trailHuntingChance = 0.1f,
            decisionSpeed = 0.8f,
            randomness = 0.1f
        };
        botConfigs.Add(cautiousBot);
        
        // Агрессивный бот
        var aggressiveBot = new IoGameBotConfig
        {
            aggressiveness = 12f,
            pathCurvature = 3f,
            collectRadius = 3f,
            trailHuntingChance = 0.5f,
            decisionSpeed = 1.5f,
            randomness = 0.3f
        };
        botConfigs.Add(aggressiveBot);
        
        // Сбалансированный бот
        var balancedBot = new IoGameBotConfig
        {
            aggressiveness = 8f,
            pathCurvature = 2f,
            collectRadius = 3.5f,
            trailHuntingChance = 0.3f,
            decisionSpeed = 1f,
            randomness = 0.2f
        };
        botConfigs.Add(balancedBot);
        
        // Хаотичный бот
        var chaoticBot = new IoGameBotConfig
        {
            aggressiveness = 10f,
            pathCurvature = 4f,
            collectRadius = 2.5f,
            trailHuntingChance = 0.4f,
            decisionSpeed = 1.8f,
            randomness = 0.5f
        };
        botConfigs.Add(chaoticBot);
        
        Debug.Log("Created 4 preset bot configurations");
    }
}