using UnityEngine;

/// <summary>
/// Адаптивный бот с системой состояний
/// Заменяет старый Enemy с более умным поведением
/// </summary>
[RequireComponent(typeof(AdaptiveBotController))]
public class AdaptiveBot : CharacterAbstract
{
    private AdaptiveBotController _controller;
    
    private void Awake()
    {
        _controller = GetComponent<AdaptiveBotController>();
        BaseInit();
    }
    
    /// <summary>
    /// Инициализация AI с привязкой к сетке
    /// </summary>
    public void InitAI(IHexGridProvider grid)
    {
        _controller.Init(grid);
    }
    
    /// <summary>
    /// Получает текущее состояние бота для отладки
    /// </summary>
    public BotState GetCurrentState()
    {
        return _controller.CurrentState;
    }
    
    /// <summary>
    /// Получает конфигурацию бота для настройки
    /// </summary>
    public BotStateConfig GetConfig()
    {
        return _controller.Config;
    }
}