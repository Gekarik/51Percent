using UnityEngine;

public interface IBotStrategy
{
    /// <summary>
    /// Уникальный идентификатор стратегии
    /// </summary>
    StrategyType Type { get; }

    /// <summary>
    /// Вычисляет приоритет (utility) этой стратегии в текущем контексте.
    /// Возвращает null если стратегия неприменима.
    /// </summary>
    float? Evaluate(BotContext context);

    /// <summary>
    /// Возвращает целевую позицию для движения
    /// </summary>
    Vector3 GetTargetPosition(BotContext context);

    /// <summary>
    /// Блокирует ли эта стратегия переключение на другие?
    /// Например, ReturnHome блокирует пока trail не закроется.
    /// </summary>
    bool IsBlocking(BotContext context);

    /// <summary>
    /// Вызывается при активации стратегии
    /// </summary>
    void OnActivate(BotContext context);

    /// <summary>
    /// Вызывается при деактивации стратегии
    /// </summary>
    void OnDeactivate(BotContext context);
}

public enum StrategyType
{
    None,
    ExpandTerritory,
    ReturnHome,
    CollectCoin,
    AttackEnemy,
    AvoidDanger,
    Wander
}
