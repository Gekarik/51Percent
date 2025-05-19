using System;
using UnityEngine;

public class Booster : MonoBehaviour, IGrabbable
{
    public event Action Collected;

    public void Collect()
    {
        Collected?.Invoke();
    }
}