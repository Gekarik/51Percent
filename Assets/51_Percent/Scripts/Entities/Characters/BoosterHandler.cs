using System;
using System.Collections;
using UnityEngine;

public class BoosterHandler : MonoBehaviour, IBoosterObservable, IBoosterLifecycle
{
    private IBoosterContext _context;
    private IBoosterEffect _activeEffect;
    private IBoosterEffect _pendingEffect;
    private Coroutine _activeCoroutine;
    private float _startTime;
    private bool _isChanging;

    public event Action BoosterChanged;
    public event Action<BoosterId> BoosterStarted;
    public event Action<BoosterId, BoosterEndReason> BoosterEnded;

    public IBoosterEffect ActiveEffect => _activeEffect;
    public bool HasActiveBooster => _activeEffect != null;
    public float RemainingTime => HasActiveBooster ? Mathf.Max(0f, _activeEffect.Duration - (Time.time - _startTime)) : 0f;
    public IBoosterEffect PendingEffect => _pendingEffect;
    public bool HasPendingBooster => _pendingEffect != null;
    // Правило «один бустер на руках»: пока есть бустер в кармане или активный — новый не берём
    public bool CanAccept => !_isChanging && !HasActiveBooster && !HasPendingBooster;
    private bool CanExecuteCommand => !_isChanging && _context != null && _context.CanAct;

    public void Init(IBoosterContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public bool TryStore(IBoosterEffect effect)
    {
        if (effect == null || !CanExecuteCommand || !CanAccept)
            return false;

        _pendingEffect = effect;
        BoosterChanged?.Invoke();
        return true;
    }

    public bool TryActivatePending()
    {
        if (!CanExecuteCommand || _pendingEffect == null || _activeEffect != null)
            return false;

        var effect = _pendingEffect;
        _pendingEffect = null;
        StartEffect(effect);
        return true;
    }

    public bool TryActivate(IBoosterEffect effect)
    {
        if (effect == null || !CanExecuteCommand || !CanAccept)
            return false;

        StartEffect(effect);
        return true;
    }

    private void StartEffect(IBoosterEffect effect)
    {
        _isChanging = true;
        try
        {
            _activeEffect = effect;
            _startTime = Time.time;
            if (effect is IEarlyConsumable consumable)
                consumable.EarlyConsumed += OnEffectEarlyConsumed;

            effect.Apply(_context);
            _activeCoroutine = StartCoroutine(RunDuration(effect));
            BoosterStarted?.Invoke(effect.BoosterId);
        }
        finally
        {
            _isChanging = false;
        }

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

    // Выбросить бустер из кармана
    public bool TryDropPending()
    {
        if (!CanExecuteCommand || _pendingEffect == null)
            return false;

        _pendingEffect = null;
        BoosterChanged?.Invoke();
        return true;
    }

    // К моменту уведомления пусты и карман, и активный слот; модификаторы уже сняты.
    public void Clear()
    {
        if (_pendingEffect == null && _activeEffect == null)
            return;

        _pendingEffect = null;
        FinishActiveEffect(BoosterEndReason.Cancelled);
        BoosterChanged?.Invoke();
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnEffectEarlyConsumed()
    {
        FinishActiveEffect(BoosterEndReason.Consumed);
        BoosterChanged?.Invoke();
    }

    private IEnumerator RunDuration(IBoosterEffect effect)
    {
        yield return new WaitForSeconds(effect.Duration);
        if (_activeEffect != effect)
            yield break;

        _activeCoroutine = null;
        FinishActiveEffect(BoosterEndReason.Expired);
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
        if (effect is IEarlyConsumable consumable)
            consumable.EarlyConsumed -= OnEffectEarlyConsumed;

        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

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
