using System;

public class WingsBoosterEffect : IBoosterEffect, IEarlyConsumable, IDeathInterceptor
{
    private readonly float _duration;
    private IBoosterContext _context;

    public BoosterId BoosterId => BoosterId.Wings;
    public float Duration => _duration;
    public event Action EarlyConsumed;

    public WingsBoosterEffect(float duration)
    {
        _duration = duration;
    }

    public void Apply(IBoosterContext context)
    {
        _context = context;
    }

    public void Remove(IBoosterContext context)
    {
        _context = null;
    }

    public bool TryEscapeDeath()
    {
        _context.EscapeToTerritory();
        EarlyConsumed?.Invoke();
        return true;
    }
}
