using TMPro;
using UnityEngine;

public class PlayerStatsView : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private TMP_Text _killsText;

    public void UpdateCoins(int value) => _coinsText.text = $"Coins: {value.ToString()}";
    public void UpdateKills(int value) => _killsText.text = $"Kills: {value.ToString()}";
}