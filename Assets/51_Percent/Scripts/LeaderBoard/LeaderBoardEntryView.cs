using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LeaderBoardEntryView : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _percentText;
    
    private Image _background;

    private void Awake()
    {
        _background = GetComponent<Image>();
    }

    public void SetData(int rank, string characterName, float percent, Color color)
    {
        if (_rankText != null)
            _rankText.text = $"{rank}";

        _nameText.text = characterName;
        _percentText.text = $"{percent:P0}";

        if (_background != null)
            _background.color = color;
    }
}
