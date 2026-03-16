using DG.Tweening;
using UnityEngine;

public class Coin : CollectibleBase
{
    private const float ScatterJumpPower = 0.5f;

    public void Scatter(Vector3 target, float duration)
    {
        State = GrabbableState.Scattering;
        transform.DOJump(target, ScatterJumpPower, 1, duration)
            .OnComplete(() => State = GrabbableState.Idle);
    }
}
