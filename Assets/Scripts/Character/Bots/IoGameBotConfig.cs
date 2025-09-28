using System;
using UnityEngine;

/// <summary>
/// Настройки поведения бота для дизайнеров
/// </summary>
[Serializable]
public class IoGameBotConfig
{
    [Header("Основные параметры")]
    [Range(1f, 20f)]
    [Tooltip("Агрессивность: как далеко от базы может отходить бот (1 = осторожно, 20 = смело)")]
    public float aggressiveness = 8f;
    
    [Range(0.5f, 5f)]
    [Tooltip("Кривизна пути: насколько изогнутые траектории (0.5 = прямо, 5 = сильные дуги)")]
    public float pathCurvature = 2f;
    
    [Range(1f, 10f)]
    [Tooltip("Радиус подбора монеток и бустеров")]
    public float collectRadius = 3f;
    
    [Header("Продвинутые настройки")]
    [Range(0f, 1f)]
    [Tooltip("Вероятность атаки вражеских трейлов (0 = избегает, 1 = всегда атакует)")]
    public float trailHuntingChance = 0.3f;
    
    [Range(0.5f, 3f)]
    [Tooltip("Скорость принятия решений (меньше = медленнее, больше = быстрее)")]
    public float decisionSpeed = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("Случайность в поведении (0 = предсказуемо, 1 = очень случайно)")]
    public float randomness = 0.2f;
}