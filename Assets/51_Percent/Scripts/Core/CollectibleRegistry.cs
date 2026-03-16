using System.Collections.Generic;

public class CollectibleRegistry : ICollectibleRegistry
{
    private readonly List<ICollectible> _collectibles = new List<ICollectible>(64);

    public IReadOnlyList<ICollectible> ActiveCollectibles => _collectibles;

    public void Register(ICollectible collectible)
    {
        _collectibles.Add(collectible);
        collectible.Collected += Unregister;
    }

    public void Unregister(ICollectible collectible)
    {
        collectible.Collected -= Unregister;
        _collectibles.Remove(collectible);
    }
}
