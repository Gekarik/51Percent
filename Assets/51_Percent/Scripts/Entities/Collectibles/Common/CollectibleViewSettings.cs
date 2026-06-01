using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleViewSettings", menuName = "51_Percent/Collectibles/View Settings")]
public class CollectibleViewSettings : ScriptableObject
{
    [Header("Idle — Bob")]
    [SerializeField] private float _bobHeight = 0.25f;
    [SerializeField] private float _bobDuration = 0.8f;
    [SerializeField] private Ease _bobEase = Ease.InOutSine;
    [SerializeField] [Range(0f, 1f)] private float _phaseRandomness = 1f;

    [Header("Idle — Rotation")]
    [SerializeField] private float _rotationDuration = 2f;

    [Header("Collect")]
    [SerializeField] private float _collectDuration = 0.25f;

    public float BobHeight => _bobHeight;
    public float BobDuration => _bobDuration;
    public Ease BobEase => _bobEase;
    public float PhaseRandomness => _phaseRandomness;
    public float RotationDuration => _rotationDuration;
    public float CollectDuration => _collectDuration;
}
