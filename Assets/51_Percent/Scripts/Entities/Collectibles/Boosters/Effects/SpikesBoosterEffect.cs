using UnityEngine;

public class SpikesBoosterEffect : IBoosterEffect
{
    public const string Id = "spikes";
    public string BoosterId => Id;
    public float Duration { get; }

    private readonly Mesh _spikedMesh;

    public SpikesBoosterEffect(float duration, Mesh spikedMesh)
    {
        Duration = duration;
        _spikedMesh = spikedMesh;
    }

    public void Apply(IBoosterContext context)
    {
        context.RegisterTrailKillResolver((owner, stepper) => (stepper, owner));
        context.SetTrailMesh(_spikedMesh);
    }

    public void Remove(IBoosterContext context)
    {
        context.UnregisterTrailKillResolver();
        context.ClearTrailMesh();
    }
}
