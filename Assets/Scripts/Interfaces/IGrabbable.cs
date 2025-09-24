using System;

public interface IGrabbable
{
    event Action<IGrabbable> Collected;
    void Collect();
    GrabbableState State { get; }
}
