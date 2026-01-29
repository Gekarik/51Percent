using UnityEngine;

public class AvoidDangerStrategy : IBotStrategy
{
    public StrategyType Type => StrategyType.AvoidDanger;

    private Vector3 _safeDirection;

    public float? Evaluate(BotContext context)
    {
        if (context.DangerousTrails.Count == 0)
            return null;

        // Вычисляем направление от опасности
        Vector3 dangerCenter = Vector3.zero;
        float closestDist = float.MaxValue;

        foreach (var hex in context.DangerousTrails)
        {
            if (hex?.Transform == null) continue;

            dangerCenter += hex.Transform.position;
            float dist = Vector3.Distance(context.Position, hex.Transform.position);
            if (dist < closestDist)
                closestDist = dist;
        }

        dangerCenter /= context.DangerousTrails.Count;

        _safeDirection = (context.Position - dangerCenter).normalized;
        _safeDirection.y = 0f;

        // Utility зависит от близости опасности и нашей склонности к риску
        float dangerProximity = 1f / Mathf.Max(closestDist, 0.5f);
        float riskAversion = 1f - context.Personality.RiskTolerance;

        float utility = dangerProximity * riskAversion * 8f;

        // Очень высокий приоритет если опасность совсем близко
        if (closestDist < 1.5f)
            utility += 10f;

        return utility > 2f ? utility : null;
    }

    public Vector3 GetTargetPosition(BotContext context)
    {
        return context.Position + _safeDirection * 4f;
    }

    public bool IsBlocking(BotContext context) => false;

    public void OnActivate(BotContext context) { }
    public void OnDeactivate(BotContext context) { }
}
