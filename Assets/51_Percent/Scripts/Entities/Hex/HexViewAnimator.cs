using DG.Tweening;
using UnityEngine;

public class HexViewAnimator
{
    private readonly Transform _hexViewTransform;
    private readonly Vector3 _originalScale;
    private readonly HexViewSettings _settings;

    private Tween _currentPulseTween;

    public HexViewAnimator(Transform hexViewTransform, HexViewSettings settings)
    {
        _hexViewTransform = hexViewTransform;
        _originalScale = _hexViewTransform.localScale;
        _settings = settings;
    }

    public void Pulse()
    {
        _currentPulseTween?.Kill();
        _currentPulseTween = _hexViewTransform
            .DOScale(_originalScale * _settings.ScaleFactor, _settings.PulseDuration)
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
