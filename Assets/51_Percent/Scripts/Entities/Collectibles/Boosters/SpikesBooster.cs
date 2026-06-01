using UnityEngine;

public class SpikesBooster : Booster
{
    [SerializeField] private float _duration = 15f;
    [SerializeField] private Mesh _spikedMesh;

    public override IBoosterEffect CreateEffect() => new SpikesBoosterEffect(_duration, _spikedMesh);
}
