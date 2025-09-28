using System;

namespace AI.Core
{
    /// <summary>
    /// Результат выполнения поведения
    /// </summary>
    public enum BehaviorResult
    {
        Success,    // Поведение успешно завершено
        Failure,    // Поведение провалилось
        Running     // Поведение выполняется
    }

    /// <summary>
    /// Типы личности бота для разных стратегий
    /// </summary>
    public enum BotPersonality
    {
        Aggressive,    // Агрессивный - активно атакует
        Defensive,     // Оборонительный - фокус на защите
        Opportunist,   // Оппортунист - использует возможности
        Territorial,   // Территориальный - расширяет территорию
        Balanced       // Сбалансированный - микс стратегий
    }

    /// <summary>
    /// Приоритеты для принятия решений
    /// </summary>
    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Состояния ИИ для отладки
    /// </summary>
    public enum AIState
    {
        Initializing,
        Thinking,
        Acting,
        Waiting,
        Dead
    }
}