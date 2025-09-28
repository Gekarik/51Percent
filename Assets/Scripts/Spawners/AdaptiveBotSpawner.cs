using UnityEngine;

/// <summary>
/// Спавнер для адаптивных ботов
/// </summary>
public class AdaptiveBotSpawner : CharacterSpawner<AdaptiveBot>
{
    [SerializeField] private int botCount = 5;
    
    [Header("Bot Configuration Presets")]
    [SerializeField] private BotStateConfig[] botConfigs;
    
    private void Start()
    {
        for (int i = 0; i < botCount; i++)
        {
            var bot = SpawnSingleCharacter();
            
            // Применяем конфигурацию, если есть
            if (botConfigs != null && botConfigs.Length > 0)
            {
                var config = botConfigs[i % botConfigs.Length];
                ApplyConfigToBot(bot, config);
            }
            
            bot.InitAI(_grid);
        }
    }
    
    /// <summary>
    /// Применяет конфигурацию к боту
    /// </summary>
    private void ApplyConfigToBot(AdaptiveBot bot, BotStateConfig sourceConfig)
    {
        var botConfig = bot.GetConfig();
        
        // Копируем настройки из preset'а
        botConfig.expandThreshold = sourceConfig.expandThreshold;
        botConfig.collectThreshold = sourceConfig.collectThreshold;
        botConfig.attackThreshold = sourceConfig.attackThreshold;
        botConfig.escapeThreshold = sourceConfig.escapeThreshold;
        
        botConfig.idleTime = sourceConfig.idleTime;
        botConfig.detectionRange = sourceConfig.detectionRange;
        botConfig.maxTrailLength = sourceConfig.maxTrailLength;
        
        botConfig.overallAggression = sourceConfig.overallAggression;
        botConfig.expandAggression = sourceConfig.expandAggression;
        botConfig.collectAggression = sourceConfig.collectAggression;
        botConfig.attackAggression = sourceConfig.attackAggression;
    }
}