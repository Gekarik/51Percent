using System.Collections.Generic;

public interface ICollectibleRegistry
{
    IReadOnlyList<ICollectible> ActiveCollectibles { get; }
    void Register(ICollectible collectible);
    void Unregister(ICollectible collectible);
}
