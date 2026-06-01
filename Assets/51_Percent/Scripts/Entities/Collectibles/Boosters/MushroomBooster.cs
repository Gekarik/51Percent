using UnityEngine;

public class MushroomBooster : Booster
{
    [SerializeField] private float _duration = 10f;
    [SerializeField] private float _speedReductionFactor = 0.3f;
    [SerializeField] private float _modelScaleFactor = 1.5f;
    [SerializeField] private float _captureWidthBonus = 1f;

    public override IBoosterEffect CreateEffect() =>
        new MushroomBoosterEffect(_duration, _speedReductionFactor, _modelScaleFactor, _captureWidthBonus);
}
