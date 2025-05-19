using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public static class HexSequenceAnimator
{
    public static Sequence AnimateBounceSequentially(IEnumerable<IHexAnimationHandler> hexes, float delayBetween = 0.1f)
    {
        var seq = DOTween.Sequence();
        foreach (var hex in hexes)
        {
            seq.AppendInterval(delayBetween).Join(hex.Bounce());
        }
        seq.Play();
        return seq;
    }

    public static void AnimateWave(IEnumerable<IHexAnimationHandler> hexes, Transform center, float waveSpeed = 1f, float maxDelay = 0.5f)
    {
        foreach (var hex in hexes)
        {
            if (hex is MonoBehaviour hexMono)
            {
                // Рассчитываем расстояние от центра до текущего гекса
                float distance = Vector3.Distance(center.position, hexMono.transform.position);

                // Рассчитываем задержку для волны
                float delay = Mathf.Clamp(distance / waveSpeed, 0, maxDelay);

                // Запускаем анимацию с задержкой
                DOVirtual.DelayedCall(delay, () => hex.Bounce());
            }
        }
    }

}