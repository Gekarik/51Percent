using UnityEngine;

public class AttackEnemyStrategy : IBotStrategy
{
    public StrategyType Type => StrategyType.AttackEnemy;

    private ICharacter _targetEnemy;

    public float? Evaluate(BotContext context)
    {
        if (context.NearbyEnemies.Count == 0)
            return null;

        // Ищем врага с активным trail (уязвимая цель)
        ICharacter bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var enemy in context.NearbyEnemies)
        {
            if (enemy == null || enemy.Transform == null)
                continue;

            if (enemy.State != CharacterState.Alive)
                continue;

            float dist = Vector3.Distance(context.Position, enemy.Transform.position);
            bool hasTrail = enemy.Conqueror?.TrailHexes?.Count > 0;

            // Уязвимые цели (с trail) гораздо привлекательнее
            float score = hasTrail ? 10f : 1f;
            score /= Mathf.Max(dist, 1f);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        if (bestTarget == null)
            return null;

        _targetEnemy = bestTarget;

        bool targetHasTrail = bestTarget.Conqueror?.TrailHexes?.Count > 0;
        float dist2 = Vector3.Distance(context.Position, bestTarget.Transform.position);
        float distanceFactor = 1f / Mathf.Max(dist2, 1f);

        // Атакуем только если враг уязвим или мы очень агрессивны
        float utility = context.Personality.Aggression * distanceFactor * 3f;

        if (targetHasTrail)
            utility *= 2f; // Двойной бонус за уязвимую цель

        // Не атакуем если у нас длинный trail (слишком рискованно)
        if (context.TrailLength > context.Personality.MaxTrailLength * 0.5f)
            utility *= 0.3f;

        return utility > 1f ? utility : null;
    }

    public Vector3 GetTargetPosition(BotContext context)
    {
        if (_targetEnemy?.Transform != null)
            return _targetEnemy.Transform.position;

        return context.Position + context.CurrentDirection * 3f;
    }

    public bool IsBlocking(BotContext context) => false;

    public void OnActivate(BotContext context) { }
    public void OnDeactivate(BotContext context) { _targetEnemy = null; }
}
