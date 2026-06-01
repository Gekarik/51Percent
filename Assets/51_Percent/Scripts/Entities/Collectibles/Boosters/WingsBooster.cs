using UnityEngine;

public class WingsBooster : Booster
{
    [SerializeField] private float _duration = 30f;

    public override IBoosterEffect CreateEffect() =>
        new WingsBoosterEffect(_duration);
}
