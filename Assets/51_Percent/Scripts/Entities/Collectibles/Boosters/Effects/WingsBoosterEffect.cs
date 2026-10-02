using System;

// Крылья: одноразовое спасение — перехватывают смерть и уносят владельца на его территорию
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

    // Спасение расходует крылья: смерть не состоялась, атака сорвалась
    public bool TryEscapeDeath()
    {
        _context.EscapeToTerritory();
        EarlyConsumed?.Invoke();
        return true;
    }
}
