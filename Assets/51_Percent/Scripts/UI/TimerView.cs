using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerView : MonoBehaviour
{
    private TextMeshProUGUI _textField;
    
    private void Awake()
    {
        _textField = GetComponent<TextMeshProUGUI>() ?? throw new System.NullReferenceException();
    }
    
    private void Update()
    {
        var time = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
        _textField.text = time.ToString(@"mm\:ss");
    }
}
