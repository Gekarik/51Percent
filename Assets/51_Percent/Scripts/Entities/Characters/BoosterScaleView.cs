using DG.Tweening;
using UnityEngine;

// Плавный рост модели от бустера. Тянется именно множитель и только через
// CharacterView: базовый масштаб знает вьюшка, и второй писатель localScale
// со своим представлением о базе оставил бы модель не того размера
public class BoosterScaleView
{
    private const float NormalFactor = 1f;

    private readonly BoosterVisualRegistry _registry;
    private readonly CharacterView _view;
    private readonly GameObject _lifetimeOwner;

    private Tween _tween;
    private float _currentFactor = NormalFactor;

    public BoosterScaleView(BoosterVisualRegistry registry, CharacterView view, GameObject lifetimeOwner)
    {
        _registry = registry;
        _view = view;
        _lifetimeOwner = lifetimeOwner;
    }

    public void Grow(BoosterId id)
    {
        if (_registry.TryGetScale(id, out BoosterScalePresentation scale))
            TweenTo(scale.Factor, scale.GrowDuration, scale.ScaleEase);
    }

    public void Restore(BoosterId id)
    {
        if (_registry.TryGetScale(id, out BoosterScalePresentation scale))
            TweenTo(NormalFactor, scale.ShrinkDuration, scale.ScaleEase);
    }

    public void Dispose()
    {
        DOTween.Kill(this);
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
            .SetLink(_lifetimeOwner)
            .SetTarget(this);
    }

    private void Apply(float factor)
    {
        _currentFactor = factor;
        _view.SetModelScale(factor);
    }
}
