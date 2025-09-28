using System;
using UnityEngine;

[Serializable]
public class BotStateConfig
{
    [Header("State Thresholds (0-100)")]
    [Range(0, 100)] public int expandThreshold = 60;
    [Range(0, 100)] public int collectThreshold = 50;
    [Range(0, 100)] public int attackThreshold = 30;
    [Range(0, 100)] public int escapeThreshold = 90;
    
    [Header("Behavior Settings")]
    [Range(0.1f, 3f)] public float idleTime = 1f;
    [Range(1f, 10f)] public float detectionRange = 5f;
    [Range(2, 30)] public int maxTrailLength = 20;
    [Range(1f, 8f)] public float movementSpeed = 3f;
    
    [Header("Aggression Levels (0-100)")]
    [Range(0, 100)] public int overallAggression = 50;
    [Range(0, 100)] public int expandAggression = 60;
    [Range(0, 100)] public int collectAggression = 40;
    [Range(0, 100)] public int attackAggression = 70;
    
    /// <summary>
    /// Проверяет, должен ли бот выполнить действие на основе агрессивности
    /// </summary>
    public bool ShouldPerformAction(BotState state, System.Random random)
    {
        int aggressionLevel = state switch
        {
            BotState.Expand => expandAggression,
            BotState.Collect => collectAggression,
            BotState.Attack => attackAggression,
            _ => overallAggression
        };
        
        return random.Next(0, 100) < aggressionLevel;
    }
    
    /// <summary>
    /// Получает порог активации для состояния
    /// </summary>
    public int GetStateThreshold(BotState state)
    {
        return state switch
        {
            BotState.Expand => expandThreshold,
            BotState.Collect => collectThreshold,
            BotState.Attack => attackThreshold,
            BotState.Escape => escapeThreshold,
            _ => 50
        };
    }
}