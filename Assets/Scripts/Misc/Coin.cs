using System;
using UnityEngine;

public class Coin : MonoBehaviour, IGrabbable
{
    public event Action Collected;
    
    public Transform Transform => transform;

    public void Collect()
    {
        Collected?.Invoke();
    }
}
