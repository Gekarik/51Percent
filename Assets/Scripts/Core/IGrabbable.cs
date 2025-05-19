using System;

public interface IGrabbable
{
    event Action Collected;
    void Collect();
}