using System;

// Активный эффект персонажа. Обычный C#-объект: правило «один бустер на руках»,
// срок действия и причина завершения проверяются без сцены и без ожидания кадров.
// Время подаёт владелец через Tick — на паузе deltaTime равен нулю, и эффект не истекает.
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
    // Правило «один бустер на руках»: подобранный применяется сразу,
    // поэтому новый не берём, пока действует текущий
    public bool CanAccept => !_isChanging && !HasActiveBooster;
    private bool CanExecuteCommand => !_isChanging && _context.CanAct;

    public bool TryActivate(IBoosterEffect effect)
    {
        if (effect == null || !CanExecuteCommand || !CanAccept)
            return false;

        StartEffect(effect);
        return true;
    }

    // Срок действия отсчитывает владелец: модель не знает ни о кадрах, ни о шкале времени
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

    // Команда ручного снятия; очистка при смерти не зависит от разрешения команд.
    public bool TryDeactivate()
    {
        if (!CanExecuteCommand || _activeEffect == null)
            return false;

        FinishActiveEffect(BoosterEndReason.Cancelled);
        BoosterChanged?.Invoke();
        return true;
    }

    // К моменту уведомления активный слот пуст, модификаторы уже сняты.
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

    // Жизненный цикл эффекта закрывает хендлер, а не сам эффект — единая точка завершения
    // независимо от причины. Причина уходит наружу: только здесь известно, почему бустер кончился
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
            // Событие после Remove и до BoosterChanged: подписчики видят согласованное состояние.
            BoosterEnded?.Invoke(effect.BoosterId, reason);
        }
        finally
        {
            _isChanging = false;
        }
    }
}
