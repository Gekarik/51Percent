using System;
using UnityEngine;

public interface ICollectible
{
    event Action<ICollectible> Collected;
    void Collect();
    CollectibleState State { get; }
    Transform Transform { get; }
}
