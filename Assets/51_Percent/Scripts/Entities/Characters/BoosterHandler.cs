using System;

public class BoosterHandler : IBoosterObservable, IBoosterLifecycle
{
    private readonly IBoosterContext _context;

    private IBoosterEffect _activeEffect;
    private float _elapsedTime;
    private bool _isChanging;

    public event Action BoosterChanged;
    public event Action<BoosterId> BoosterStarted;
    public event Action<BoosterId, BoosterEndReason> BoosterEnded;

    public BoosterHandler(IBoosterContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IBoosterEffect ActiveEffect => _activeEffect;
    public bool HasActiveBooster => _activeEffect != null;
    public float RemainingTime => HasActiveBooster ? Math.Max(0f, _activeEffect.Duration - _elapsedTime) : 0f;
    public bool CanAccept => !_isChanging && !HasActiveBooster;
    private bool CanExecuteCommand => !_isChanging && _context.CanAct;

    public bool TryActivate(IBoosterEffect effect)
    {
        if (effect == null || !CanExecuteCommand || !CanAccept)
            return false;

        StartEffect(effect);
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (_activeEffect == null || deltaTime <= 0f)
            return;

        _elapsedTime += deltaTime;

        if (_elapsedTime < _activeEffect.Duration)
            return;

        FinishActiveEffect(BoosterEndReason.Expired);
        BoosterChanged?.Invoke();
    }

    public bool TryDeactivate()
    {
        if (!CanExecuteCommand || _activeEffect == null)
            return false;

        FinishActiveEffect(BoosterEndReason.Cancelled);
        BoosterChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (_activeEffect == null)
            return;

        FinishActiveEffect(BoosterEndReason.Cancelled);
        BoosterChanged?.Invoke();
    }

    private void StartEffect(IBoosterEffect effect)
    {
        _isChanging = true;
        try
        {
            _activeEffect = effect;
            _elapsedTime = 0f;
            if (effect is IEarlyConsumable consumable)
                consumable.EarlyConsumed += OnEffectEarlyConsumed;

            effect.Apply(_context);
            BoosterStarted?.Invoke(effect.BoosterId);
        }
        finally
        {
            _isChanging = false;
        }

        BoosterChanged?.Invoke();
    }

    private void OnEffectEarlyConsumed()
    {
        FinishActiveEffect(BoosterEndReason.Consumed);
        BoosterChanged?.Invoke();
    }

    private void FinishActiveEffect(BoosterEndReason reason)
    {
        if (_activeEffect == null)
            return;

        var effect = _activeEffect;
        _activeEffect = null;
        _elapsedTime = 0f;
        if (effect is IEarlyConsumable consumable)
            consumable.EarlyConsumed -= OnEffectEarlyConsumed;

        _isChanging = true;
        try
        {
            effect.Remove(_context);
            BoosterEnded?.Invoke(effect.BoosterId, reason);
        }
        finally
        {
            _isChanging = false;
        }
    }
}
