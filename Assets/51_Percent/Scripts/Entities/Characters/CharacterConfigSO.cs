using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "51_Percent/Characters/CharacterConfig")]
public class CharacterConfigSO : ScriptableObject
{
    [SerializeField] private float _baseSpeed = 5f;
    [SerializeField] private float _baseCaptureWidth = 1f;
    [SerializeField] private float _rotationSpeed = 720f;

    // Срок окна приземления принадлежит состоянию персонажа, а не его презентации
    [SerializeField, Min(0.01f)] private float _landingDuration = 0.75f;

    public float BaseSpeed => _baseSpeed;
    public float BaseCaptureWidth => _baseCaptureWidth;
    public float RotationSpeed => _rotationSpeed;
    public float LandingDuration => _landingDuration;
}
