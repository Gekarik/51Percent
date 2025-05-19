using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class WalletView : MonoBehaviour
{
    [SerializeField] private Wallet _wallet;
    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _textMeshPro.text = $"Coins: {_wallet.CoinAmount.ToString()}";
    }

    private void OnEnable()
    {
        _wallet.AmountChanged += OnAmountChanged;
    }

    private void OnAmountChanged()
    {
        _textMeshPro.text = $"Coins: {_wallet.CoinAmount.ToString()}";
    }

    private void OnDisable()
    {
        _wallet.AmountChanged -= OnAmountChanged;
    }
}