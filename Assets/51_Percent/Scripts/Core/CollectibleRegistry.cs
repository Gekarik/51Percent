using System;
using System.Collections.Generic;

public class CollectibleRegistry : ICollectibleRegistry
{
    private readonly List<ICollectible> _coins = new List<ICollectible>(64);
    private readonly List<ICollectible> _boosters = new List<ICollectible>(16);

    public IReadOnlyList<ICollectible> Coins => _coins;
    public IReadOnlyList<ICollectible> Boosters => _boosters;

    public void Register(ICollectible collectible, CollectibleKind kind)
    {
        GetList(kind).Add(collectible);
        collectible.Collected += Unregister;
    }

    public void Unregister(ICollectible collectible)
    {
        collectible.Collected -= Unregister;
        _coins.Remove(collectible);
        _boosters.Remove(collectible);
    }

    private List<ICollectible> GetList(CollectibleKind kind) => kind switch
    {
        CollectibleKind.Coin => _coins,
        CollectibleKind.Booster => _boosters,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
