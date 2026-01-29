using System;
using UnityEngine;

public interface ICollectible
{
    event Action<ICollectible> Collected;
    void Collect();
    GrabbableState State { get; }
    Transform Transform { get; }
}
