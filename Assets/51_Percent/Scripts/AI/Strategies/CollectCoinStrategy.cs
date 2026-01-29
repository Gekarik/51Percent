using UnityEngine;

public class CollectCoinStrategy : IBotStrategy
{
    public StrategyType Type => StrategyType.CollectCoin;

    private Transform _targetCoin;

    public float? Evaluate(BotContext context)
    {
        if (context.NearbyCoins.Count == 0)
            return null;

        // Находим ближайшую монетку
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var coin in context.NearbyCoins)
        {
            if (coin == null) continue;

            float dist = Vector3.Distance(context.Position, coin.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = coin;
            }
        }

        if (nearest == null)
            return null;

        _targetCoin = nearest;

        // Utility зависит от жадности и расстояния
        float distanceFactor = 1f / Mathf.Max(nearestDist, 1f);
        float utility = context.Personality.Greed * distanceFactor * 4f;

        // Снижаем приоритет если trail длинный (лучше вернуться домой)
        float trailPenalty = context.TrailLength * 0.3f;
        utility -= trailPenalty;

        return utility > 0 ? utility : null;
    }

    public Vector3 GetTargetPosition(BotContext context)
    {
        if (_targetCoin != null)
            return _targetCoin.position;

        // Fallback - ищем заново
        foreach (var coin in context.NearbyCoins)
        {
            if (coin != null)
                return coin.position;
        }

        return context.Position + context.CurrentDirection * 3f;
    }

    public bool IsBlocking(BotContext context) => false;

    public void OnActivate(BotContext context) { }
    public void OnDeactivate(BotContext context) { _targetCoin = null; }
}
