using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Hex/Animation Settings")]
public class AnimationSettingsSO : ScriptableObject
{
    [Header("Bounce")]
    public float bounceHeight = 1f;
    public float bounceDuration = 0.5f;
    public int bounceJumps = 1;
    public Ease bounceEase = Ease.OutQuad;

    [Header("Stretch")]
    public Vector3 stretchScale = new Vector3(1.2f, 0.8f, 1f);
    public float stretchDuration = 0.2f;
    public Ease stretchEaseUp = Ease.OutQuad;
    public Ease stretchEaseDown = Ease.InQuad;
}
