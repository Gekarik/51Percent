using DG.Tweening;
using UnityEngine;

public class HexViewAnimator
{
    [SerializeField] private float _scaleFactor = 1.2f;
    [SerializeField] private float _pulseDuration = 0.15f;

    private readonly Transform _hexViewTransform;
    private readonly Vector3 _originalScale;

    private Tween _currentPulseTween;

    public HexViewAnimator(Transform hexViewTransform)
    {
        _hexViewTransform = hexViewTransform;
        _originalScale = _hexViewTransform.localScale;
    }

    public void Pulse()
    {
        _currentPulseTween = _hexViewTransform
            .DOScale(_originalScale * _scaleFactor, _pulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .OnKill(() => _hexViewTransform.localScale = _originalScale)
            .OnComplete(() => _hexViewTransform.localScale = _originalScale);
    }

    public void Reset()
    {
        _currentPulseTween?.Kill();
        _hexViewTransform.localScale = _originalScale;
    }
}
