using System;
using UnityEngine;

public class Coin : MonoBehaviour, IGrabbable
{
    public event Action Collected;

    public void Collect()
    {
        Collected?.Invoke();
    }
}