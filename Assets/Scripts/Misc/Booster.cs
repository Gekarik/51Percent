using System;
using UnityEngine;

public class Booster : MonoBehaviour, IGrabbable
{
    public event Action Collected;
    public Transform Transform => transform;

    public void Collect()
    {
        Collected?.Invoke();
    }
}
