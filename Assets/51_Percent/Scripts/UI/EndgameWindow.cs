using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndgameWindow : MonoBehaviour
{
    [SerializeField] private Image _headerBanner;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private TMP_Text _winnerNameText;
    [SerializeField] private TMP_Text _territoryText;
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;

    public event Action RestartClicked;
    public event Action ExitClicked;

    private void OnEnable()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDisable()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _exitButton.onClick.RemoveListener(OnExitClicked);
    }

    public void Show(string winnerName, Color winnerColor, bool isVictory, float territory, int kills, int coins)
    {
        _headerBanner.color = winnerColor;
        _resultText.text = isVictory ? "Победа!" : "Поражение!";
        _winnerNameText.text = winnerName;
        _territoryText.text = $"{territory:P0}";
        _killsText.text = $"{kills}";
        _coinsText.text = $"{coins}";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnRestartClicked() => RestartClicked?.Invoke();
    private void OnExitClicked() => ExitClicked?.Invoke();
}
