using UnityEngine;

public class ReturnHomeStrategy : IBotStrategy
{
    private const int MinTrailToReturn = 3;

    public StrategyType Type => StrategyType.ReturnHome;

    public float? Evaluate(BotContext context)
    {
        if (context.TrailLength < MinTrailToReturn)
            return null;

        var territory = context.Territory;
        if (territory == null || territory.Count == 0)
            return null;

        float trailRatio = (float)context.TrailLength / context.Personality.MaxTrailLength;
        float riskAversion = 1f - context.Personality.RiskTolerance;

        float utility = trailRatio * riskAversion * 5f;

        if (trailRatio >= 0.7f)
            utility += 5f;

        if (trailRatio >= 0.9f)
            utility += 10f;

        return utility;
    }

    public Vector3 GetTargetPosition(BotContext context)
    {
        var bestHex = FindLoopClosingHex(context);

        if (bestHex?.Transform != null)
            return bestHex.Transform.position;

        return context.GetTerritoryCenter();
    }

    /// <summary>
    /// Ищем домашний гекс, который замкнёт петлю — предпочитаем гексы
    /// "впереди" по текущему направлению движения, а не ближайший
    /// </summary>
    private IHex FindLoopClosingHex(BotContext context)
    {
        var territory = context.Territory;
        if (territory == null || territory.Count == 0)
            return null;

        Vector3 moveDir = context.CurrentDirection;
        bool hasDirection = moveDir.sqrMagnitude > 0.01f;

        IHex best = null;
        float bestScore = float.MinValue;

        foreach (var hex in territory)
        {
            if (hex?.Transform == null) continue;

            Vector3 toHex = hex.Transform.position - context.Position;
            toHex.y = 0f;
            float dist = toHex.magnitude;

            if (dist < 0.1f) continue;

            float score;

            if (hasDirection)
            {
                float dot = Vector3.Dot(toHex.normalized, moveDir.normalized);
                // Баланс: ближе + впереди по направлению движения
                score = dot * 3f - dist * 0.3f;
            }
            else
            {
                // Нет направления — просто ближайший
                score = -dist;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = hex;
            }
        }

        return best ?? context.GetNearestHomeHex();
    }

    public bool IsBlocking(BotContext context)
    {
        return context.HasTrail;
    }

    public void OnActivate(BotContext context)
    {
        Debug.Log($"[{context.BotTransform.name}] ReturnHome ACTIVATED. Trail={context.TrailLength}");
    }

    public void OnDeactivate(BotContext context)
    {
        Debug.Log($"[{context.BotTransform.name}] ReturnHome DEACTIVATED. Trail={context.TrailLength}");
    }
}
