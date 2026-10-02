using System;
using UnityEngine;

public interface ICollectible
{
    event Action<ICollectible> Consumed;

    event Action<ICollectible> Collected;
    void Collect();
    bool TryApplyTo(ICollectibleConsumer consumer);
    CollectibleState State { get; }
    Transform Transform { get; }
}
