using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseWindow : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _settingsButton;

    public event Action ContinueClicked;
    public event Action RestartClicked;
    public event Action ExitClicked;
    public event Action SettingsClicked;

    private void OnEnable()
    {
        _continueButton.onClick.AddListener(OnContinueClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
        _exitButton.onClick.AddListener(OnExitClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDisable()
    {
        _continueButton.onClick.RemoveListener(OnContinueClicked);
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _exitButton.onClick.RemoveListener(OnExitClicked);
        _settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnContinueClicked() => ContinueClicked?.Invoke();
    private void OnRestartClicked() => RestartClicked?.Invoke();
    private void OnExitClicked() => ExitClicked?.Invoke();
    private void OnSettingsClicked() => SettingsClicked?.Invoke();
}
