using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "51_Percent/Characters/CharacterConfig")]
public class CharacterConfigSO : ScriptableObject
{
    [SerializeField] private float _baseSpeed = 5f;
    [SerializeField] private float _baseCaptureWidth = 1f;
    [SerializeField] private float _rotationSpeed = 720f;

    public float BaseSpeed => _baseSpeed;
    public float BaseCaptureWidth => _baseCaptureWidth;
    public float RotationSpeed => _rotationSpeed;
}
