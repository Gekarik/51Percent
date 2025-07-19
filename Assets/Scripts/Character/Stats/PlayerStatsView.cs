using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class PlayerStatsView : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private TMP_Text _killsText;

    private Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    public void SetCamera(Camera cam)
    {
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = cam;
    }

    public void UpdateCoins(int value) => _coinsText.text = $"Coins: {value.ToString()}";
    public void UpdateKills(int value) => _killsText.text = $"Kills: {value.ToString()}";
}