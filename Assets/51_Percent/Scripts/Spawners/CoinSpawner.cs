using UnityEngine;

public class CoinSpawner : PooledSpawner<Coin>, ICoinScatterer
{
    protected override CollectibleKind Kind => CollectibleKind.Coin;

    [Header("Scatter")]
    [SerializeField] private float _scatterRadius = 1.5f;
    [SerializeField] private float _scatterDuration = 0.4f;

    public void ScatterCoins(Vector3 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var coin = SpawnTyped(origin);
            var offset = Random.insideUnitCircle * _scatterRadius;
            var target = origin + new Vector3(offset.x, 0f, offset.y);
            coin.Scatter(target, _scatterDuration);
        }
    }
}
