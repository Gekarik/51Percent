using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoosterHUD : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _timerText;

    private PlayerBoosterHudPresenter _presenter;

    public void Bind(PlayerBoosterHudPresenter presenter)
    {
        _presenter = presenter;
    }

    private void Update()
    {
        _presenter?.Tick();
    }

    public void Show(Sprite icon)
    {
        _container.SetActive(true);
        _iconImage.sprite = icon;
    }

    public void Hide()
    {
        _container.SetActive(false);
    }

    public void SetTimer(string text)
    {
        _timerText.text = text;
    }
}
