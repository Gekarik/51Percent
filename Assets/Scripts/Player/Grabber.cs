using System;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public event Action<Coin> CoinCollected;
    public event Action<Booster> BoosterCollected;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent(out IGrabbable item))
        {
            switch (item)
            {
                case Coin coin:
                    CoinCollected?.Invoke(coin);
                    break;

                case Booster booster:
                    BoosterCollected?.Invoke(booster);
                    break;
            }

            item.Collect();
        }
    }
}
