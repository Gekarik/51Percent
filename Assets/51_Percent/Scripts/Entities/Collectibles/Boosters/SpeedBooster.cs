using UnityEngine;

public class SpeedBooster : Booster
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _speedBonus = 0.5f;

    public override IBoosterEffect CreateEffect() => new SpeedBoosterEffect(_duration, _speedBonus);
}
