using System;

public class WingsBoosterEffect : IBoosterEffect, IEarlyConsumable
{
    private readonly float _duration;
    private IBoosterContext _context;

    public string BoosterId => "wings";
    public float Duration => _duration;
    public event Action EarlyConsumed;

    public WingsBoosterEffect(float duration)
    {
        _duration = duration;
    }

    public void Apply(IBoosterContext context)
    {
        _context = context;
        context.Died += OnDied;
    }

    public void Remove(IBoosterContext context)
    {
        context.Died -= OnDied;
        _context = null;
    }

    private void OnDied()
    {
        _context.Died -= OnDied;
        _context.RequestRespawn();
        _context = null;
        EarlyConsumed?.Invoke();
    }
}
