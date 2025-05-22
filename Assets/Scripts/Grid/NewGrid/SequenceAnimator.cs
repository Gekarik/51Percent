using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public static class SequenceAnimator
{
    private static Sequence AnimateBounceSequentially(IEnumerable<IHexAnimationHandler> hexes, float delayBetween = 0.1f)
    {
        var seq = DOTween.Sequence();
        foreach (var h in hexes)
        {
            seq.AppendInterval(delayBetween);
            seq.Join(h.Bounce());
        }
        return seq;
    }

    public static void AnimateWave(IEnumerable<IHexAnimationHandler> hexes, UnityEngine.Transform center, float waveSpeed = 1f, float maxDelay = 0.5f)
    {
        foreach (var h in hexes)
        {
            if (h is UnityEngine.MonoBehaviour mb)
            {
                float dist = Vector3.Distance(center.position, mb.transform.position);
                float delay = Mathf.Clamp(dist / waveSpeed, 0, maxDelay);
                DOVirtual.DelayedCall(delay, () => h.Bounce()).SetUpdate(true);
            }
        }
    }
}
