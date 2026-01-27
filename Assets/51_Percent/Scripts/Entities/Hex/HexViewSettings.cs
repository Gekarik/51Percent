using UnityEngine;

[CreateAssetMenu(fileName = "HexAnimationSettings", menuName = "Hex/AnimationSettings")]
public class HexViewSettings : ScriptableObject
{
    [Header("Pulse")]
    [SerializeField] private float _scaleFactor = 1.2f;
    [SerializeField] private float _pulseDuration = 0.15f;

    [Header("Colorizer")]
    [SerializeField] private float _colorizeDuration = 0.1f;
    [SerializeField] private float _resetColorDuration = 0.1f;

    public float ScaleFactor => _scaleFactor;
    public float PulseDuration => _pulseDuration;
    public float ColorizeDuration => _colorizeDuration;
    public float ResetColorDuration => _resetColorDuration;
}
