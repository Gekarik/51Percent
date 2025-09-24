using DG.Tweening;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grabber : MonoBehaviour
{
    public event Action<IGrabbable> ItemCollected;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent(out IGrabbable item) && item.State == GrabbableState.Idle)
        {
            ItemCollected?.Invoke(item);
            item.Collect();
        }
    }
}
