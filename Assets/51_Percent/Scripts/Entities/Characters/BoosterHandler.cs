using System;
using System.Collections;
using UnityEngine;

public class BoosterHandler : MonoBehaviour, IBoosterObservable
{
    private IBoosterContext _context;
    private IBoosterEffect _activeEffect;
    private IBoosterEffect _pendingEffect;
    private Coroutine _activeCoroutine;
    private float _startTime;

    public event Action BoosterChanged;
    public IBoosterEffect ActiveEffect => _activeEffect;
    public bool HasActiveBooster => _activeEffect != null;
    public float RemainingTime => HasActiveBooster ? Mathf.Max(0f, _activeEffect.Duration - (Time.time - _startTime)) : 0f;
    public IBoosterEffect PendingEffect => _pendingEffect;
    public bool HasPendingBooster => _pendingEffect != null;

    public void Init(IBoosterContext context)
    {
        _context = context;
    }

    public void Store(IBoosterEffect effect)
    {
        _pendingEffect = effect;
        BoosterChanged?.Invoke();
    }

    public void ActivatePending()
    {
        if (_pendingEffect == null) return;
        var effect = _pendingEffect;
        _pendingEffect = null;
        Activate(effect);
    }

    public void Activate(IBoosterEffect effect)
    {
        if (_activeEffect != null)
        {
            if (_activeEffect is IEarlyConsumable previousConsumable)
                previousConsumable.EarlyConsumed -= OnEffectEarlyConsumed;
            _activeEffect.Remove(_context);
            StopCoroutine(_activeCoroutine);
        }

        _activeEffect = effect;
        _startTime = Time.time;
        if (_activeEffect is IEarlyConsumable consumable)
            consumable.EarlyConsumed += OnEffectEarlyConsumed;
        _activeEffect.Apply(_context);
        _activeCoroutine = StartCoroutine(RunDuration(effect));
        BoosterChanged?.Invoke();
    }

    private void OnEffectEarlyConsumed()
    {
        if (_activeEffect is IEarlyConsumable consumable)
            consumable.EarlyConsumed -= OnEffectEarlyConsumed;
        StopCoroutine(_activeCoroutine);
        _activeEffect = null;
        BoosterChanged?.Invoke();
    }

    private IEnumerator RunDuration(IBoosterEffect effect)
    {
        yield return new WaitForSeconds(effect.Duration);
        effect.Remove(_context);
        _activeEffect = null;
        BoosterChanged?.Invoke();
    }
}
