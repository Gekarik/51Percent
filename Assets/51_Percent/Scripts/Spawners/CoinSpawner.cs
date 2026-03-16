using UnityEngine;

public class CoinSpawner : ObjectSpawner<Coin>
{
    [SerializeField] private float _scatterRadius = 1.5f;
    [SerializeField] private float _scatterDuration = 0.4f;

    public void ScatterCoins(Vector3 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var coin = SpawnAtPosition(origin);
            var offset = Random.insideUnitCircle * _scatterRadius;
            var target = origin + new Vector3(offset.x, 0f, offset.y);
            coin.Scatter(target, _scatterDuration);
        }
    }
}
