using System;
using UnityEngine;

public class SpikesBoosterEffect : IBoosterEffect, IEarlyConsumable, ITrailKillModifier
{
    public BoosterId BoosterId => BoosterId.Spikes;
    public float Duration { get; }
    public event Action EarlyConsumed;

    private readonly Mesh _spikedMesh;

    public SpikesBoosterEffect(float duration, Mesh spikedMesh)
    {
        Duration = duration;
        _spikedMesh = spikedMesh;
    }

    public void Apply(IBoosterContext context)
    {
        context.SetTrailMesh(_spikedMesh);
    }

    public void Remove(IBoosterContext context)
    {
        context.ClearTrailMesh();
    }

    public (ICharacter victim, ICharacter killer) ResolveTrailKill(ICharacter trailOwner, ICharacter stepper)
    {
        EarlyConsumed?.Invoke();
        return (stepper, trailOwner);
    }
}
