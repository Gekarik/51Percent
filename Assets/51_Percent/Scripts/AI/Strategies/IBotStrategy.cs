using UnityEngine;

public interface IBotStrategy
{
    StrategyType Type { get; }
    float? Evaluate(BotContext context);
    Vector3 GetTargetPosition(BotContext context);
    bool IsBlocking(BotContext context);
    void OnActivate(BotContext context);
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
