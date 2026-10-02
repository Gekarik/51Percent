using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grabber : MonoBehaviour
{
    public event Action<ICollectible> ItemDetected;

    private void OnTriggerEnter(Collider collider)
    {
        if (!isActiveAndEnabled)
            return;

        if (collider.gameObject.TryGetComponent(out ICollectible item) && item.State == CollectibleState.Idle)
            ItemDetected?.Invoke(item);
    }
}
