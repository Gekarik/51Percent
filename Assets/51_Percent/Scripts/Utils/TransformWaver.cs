using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
using UnityEngine;

public class TransformWaver : IWaveAnimator
{
    private float _animationHeight = 0.6f;
    private float _totalWaveDuration = 0.8f;
    private float _overlapFactor = 0.5f; // 0 = полное наложение, 1 ~ последовательный

    private readonly WaveDelayResolver _delayResolver;
    private readonly Dictionary<Transform, Vector3> _originalPositions = new();
    private readonly List<Tween> _activeTweens = new();

    public TransformWaver(WaveDelayResolver delayResolver)
    {
        _delayResolver = delayResolver ?? throw new ArgumentNullException(nameof(delayResolver));
    }

    private float GetElementDuration()
    {
        float clamped = Mathf.Clamp01(_overlapFactor);
        float duration = _totalWaveDuration * (1f - clamped);

        return duration > 0f ? duration : Mathf.Epsilon;
    }

    public void Wave(IReadOnlyCollection<Transform> transforms, Vector3 origin)
    {
        var elements = transforms.ToArray();

        if (elements.Length == 0)
            return;

        SaveOriginalPositions(elements);

        float elementDuration = GetElementDuration();
        float[] delays = _delayResolver.Resolve(elements, origin, _totalWaveDuration - elementDuration);

        StartWaveAnimation(elements, elementDuration, delays);
    }

    private void SaveOriginalPositions(Transform[] transforms)
    {
        foreach (var transform in transforms)
        {
            if (_originalPositions.ContainsKey(transform) == false)
                _originalPositions[transform] = transform.localPosition;
        }
    }

    private void StartWaveAnimation(Transform[] elements, float elementDuration, float[] delays)
    {
        float driver = 0f;

        var tween = DOTween.To(
            () => driver,
            value => {
                driver = value;
                UpdateWave(elements, driver, elementDuration, delays);
            }, 1f, _totalWaveDuration).SetEase(Ease.Linear);

        tween.OnComplete(() => RestorePositions(elements, tween)).OnKill(() => RestorePositions(elements, tween));
        _activeTweens.Add(tween);
    }

    private void UpdateWave(Transform[] elements, float driver, float elementDuration, float[] delays)
    {
        float elapsedTime = driver * _totalWaveDuration;

        for (int i = 0; i < elements.Length; i++)
        {
            var tf = elements[i];
            float localTime = (elapsedTime - delays[i]) / elementDuration;

            if (_originalPositions.TryGetValue(tf, out var origPos))
                tf.localPosition = origPos + Vector3.up * CalculateOffset(localTime);
        }
    }

    private float CalculateOffset(float localTime)
    {
        return (localTime >= 0f && localTime <= 1f) ? Mathf.Sin(localTime * Mathf.PI) * _animationHeight : 0f;
    }

    private void RestorePositions(Transform[] elements, Tween tween)
    {
        foreach (var tf in elements)
        {
            if (_originalPositions.TryGetValue(tf, out var origPos))
            {
                tf.localPosition = origPos;
                _originalPositions.Remove(tf);
            }
        }

        _activeTweens.Remove(tween);
    }
}
