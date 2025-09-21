using DG.Tweening;
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

                default:
                    throw new NotImplementedException();
            }

            item.Collect();
        }
    }

    private void CollectAnimation(Transform item)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Join(item.DOScale(Vector3.zero, 0.5f)).Join(item.DOMove(transform.position, 0.5f));
    }
}
