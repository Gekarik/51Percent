using System;
using UnityEngine;

public interface IGrabbable
{
    event Action<IGrabbable> Collected;
    void Collect();
    GrabbableState State { get; }
    Transform Transform { get; }
}
