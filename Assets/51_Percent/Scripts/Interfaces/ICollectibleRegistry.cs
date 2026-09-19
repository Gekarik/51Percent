using System.Collections.Generic;

public interface ICollectibleRegistry
{
    IReadOnlyList<ICollectible> Coins { get; }
    IReadOnlyList<ICollectible> Boosters { get; }
    void Register(ICollectible collectible, CollectibleKind kind);
    void Unregister(ICollectible collectible);
}
