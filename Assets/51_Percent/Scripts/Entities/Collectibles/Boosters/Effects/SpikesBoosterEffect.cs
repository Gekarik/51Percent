using System;
using UnityEngine;

// Шипы: одноразовая ловушка — разворачивают одно убийство по трейлу и гаснут
public class SpikesBoosterEffect : IBoosterEffect, IEarlyConsumable
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
        context.RegisterTrailKillResolver(ReverseKill);
        context.SetTrailMesh(_spikedMesh);
    }

    public void Remove(IBoosterContext context)
    {
        context.UnregisterTrailKillResolver();
        context.ClearTrailMesh();
    }

    private (ICharacter victim, ICharacter killer) ReverseKill(ICharacter owner, ICharacter stepper)
    {
        EarlyConsumed?.Invoke();
        return (stepper, owner);
    }
}
