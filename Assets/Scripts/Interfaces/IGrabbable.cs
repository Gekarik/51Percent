using System;
using UnityEngine;

public interface IGrabbable
{
    event Action Collected;
    void Collect();
    Transform Transform { get; }
}