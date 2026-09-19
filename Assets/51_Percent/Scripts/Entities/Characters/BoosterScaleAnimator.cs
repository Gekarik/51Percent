using System;
using DG.Tweening;
using UnityEngine;

// Плавный рост модели от бустера. Тянется именно множитель и только через
// CharacterView: базовый масштаб знает вьюшка, и второй писатель localScale
// со своим представлением о базе оставил бы модель не того размера
public class BoosterScaleAnimator : MonoBehaviour
{
    private const float NormalFactor = 1f;

    [SerializeField] private BoosterVisualRegistry _registry;

    private IBoosterLifecycle _lifecycle;
    private CharacterView _view;
    private Tween _tween;
    private float _currentFactor = NormalFactor;

    public void Init(IBoosterLifecycle lifecycle, CharacterView view)
    {
        if (_registry == null)
            throw new InvalidOperationException($"[{GetType().Name}] _registry не назначен на '{name}'");

        _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        _view = view != null ? view : throw new ArgumentNullException(nameof(view));

        _lifecycle.BoosterStarted += OnBoosterStarted;
        _lifecycle.BoosterEnded += OnBoosterEnded;
    }

    private void OnDestroy()
    {
        if (_lifecycle != null)
        {
            _lifecycle.BoosterStarted -= OnBoosterStarted;
            _lifecycle.BoosterEnded -= OnBoosterEnded;
        }

        DOTween.Kill(this);
    }

    private void OnBoosterStarted(BoosterId id)
    {
        if (_registry.TryGetScale(id, out BoosterScalePresentation scale))
            TweenTo(scale.Factor, scale.GrowDuration, scale.ScaleEase);
    }

    private void OnBoosterEnded(BoosterId id, BoosterEndReason reason)
    {
        if (_registry.TryGetScale(id, out BoosterScalePresentation scale))
            TweenTo(NormalFactor, scale.ShrinkDuration, scale.ScaleEase);
    }

    // Тянем от текущего множителя, а не от единицы: разворот на полпути должен быть непрерывным
    private void TweenTo(float target, float duration, Ease ease)
    {
        _tween?.Kill();

        if (duration <= 0f)
        {
            Apply(target);
            return;
        }

        _tween = DOVirtual.Float(_currentFactor, target, duration, Apply)
            .SetEase(ease)
            .SetLink(gameObject)
            .SetTarget(this);
    }

    private void Apply(float factor)
    {
        _currentFactor = factor;
        _view.SetModelScale(factor);
    }
}
