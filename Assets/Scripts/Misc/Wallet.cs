using System;
using UnityEngine;

[RequireComponent(typeof(Grabber))]
public class Wallet : MonoBehaviour
{
    public event Action AmountChanged;
    private Grabber _grabber;
    public int CoinAmount { get; private set; }

    private void Awake()
    {
        _grabber = GetComponent<Grabber>();
    }

    private void OnEnable()
    {
        _grabber.CoinCollected += OnCollected;
    }

    private void OnCollected(Coin coin)
    {
        CoinAmount++;
        AmountChanged?.Invoke();
    }

    private void OnDisable()
    {
        _grabber.CoinCollected -= OnCollected;
    }
}
