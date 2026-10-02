using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Правила активного эффекта без сцены и без ожидания кадров:
// модель получает время через Tick, поэтому срок действия проверяется напрямую.
public class BoosterHandlerTests
{
    private const float Duration = 10f;

    private readonly List<BoosterEndReason> _endReasons = new List<BoosterEndReason>();

    private FakeContext _context;
    private BoosterHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _endReasons.Clear();
        _context = new FakeContext();
        _handler = new BoosterHandler(_context);
        _handler.BoosterEnded += (id, reason) => _endReasons.Add(reason);
    }

    [Test]
    public void SecondBooster_WhileOneIsActive_IsRejected()
    {
        Assert.That(_handler.TryActivate(new FakeEffect(Duration)), Is.True);
        Assert.That(_handler.CanAccept, Is.False, "Руки свободны при действующем эффекте.");
        Assert.That(_handler.TryActivate(new FakeEffect(Duration)), Is.False,
            "Бустер активировался поверх действующего.");
    }

    [Test]
    public void ActiveBooster_ExpiresExactlyAfterItsDuration()
    {
        var effect = new FakeEffect(Duration);
        _handler.TryActivate(effect);

        _handler.Tick(Duration - 0.1f);

        Assert.That(_handler.HasActiveBooster, Is.True, "Эффект снят раньше срока.");
        Assert.That(_handler.RemainingTime, Is.EqualTo(0.1f).Within(0.0001f));

        _handler.Tick(0.1f);

        Assert.That(_handler.HasActiveBooster, Is.False, "Эффект пережил свой срок.");
        Assert.That(effect.Removed, Is.EqualTo(1));
        Assert.That(_endReasons, Is.EqualTo(new[] { BoosterEndReason.Expired }));
    }

    [Test]
    public void PausedTime_DoesNotConsumeDuration()
    {
        _handler.TryActivate(new FakeEffect(Duration));
        float before = _handler.RemainingTime;

        for (int i = 0; i < 100; i++)
            _handler.Tick(0f);

        Assert.That(_handler.RemainingTime, Is.EqualTo(before).Within(0.0001f));
        Assert.That(_handler.HasActiveBooster, Is.True);
    }

    [Test]
    public void Commands_AreRejected_WhileCharacterCannotAct()
    {
        _context.CanAct = false;

        Assert.That(_handler.TryActivate(new FakeEffect(Duration)), Is.False,
            "Бустер запущен, когда персонаж не может действовать.");

        _context.CanAct = true;
        _handler.TryActivate(new FakeEffect(Duration));
        _context.CanAct = false;

        Assert.That(_handler.TryDeactivate(), Is.False,
            "Эффект снят командой, когда персонаж не может действовать.");
    }

    // Очистка при смерти не зависит от разрешения команд: персонаж уже не может действовать
    [Test]
    public void Clear_RemovesActiveEffect_EvenWhenCharacterCannotAct()
    {
        var active = new FakeEffect(Duration);
        _handler.TryActivate(active);
        _context.CanAct = false;

        _handler.Clear();

        Assert.That(_handler.HasActiveBooster, Is.False);
        Assert.That(active.Removed, Is.EqualTo(1));
        Assert.That(_endReasons, Is.EqualTo(new[] { BoosterEndReason.Cancelled }));
    }

    [Test]
    public void EarlyConsumedEffect_EndsWithConsumedReason()
    {
        var effect = new ConsumableEffect(Duration);
        _handler.TryActivate(effect);

        effect.Consume();

        Assert.That(_handler.HasActiveBooster, Is.False);
        Assert.That(_endReasons, Is.EqualTo(new[] { BoosterEndReason.Consumed }));
    }

    private sealed class FakeContext : IBoosterContext
    {
        public bool CanAct { get; set; } = true;
        public CharacterStats Stats { get; } = new CharacterStats();

        public void EscapeToTerritory() { }
        public void SetTrailMesh(Mesh mesh) { }
        public void ClearTrailMesh() { }
    }

    private class FakeEffect : IBoosterEffect
    {
        public FakeEffect(float duration) => Duration = duration;

        public BoosterId BoosterId => BoosterId.Speed;
        public float Duration { get; }
        public int Removed { get; private set; }

        public void Apply(IBoosterContext context) { }
        public void Remove(IBoosterContext context) => Removed++;
    }

    private sealed class ConsumableEffect : FakeEffect, IEarlyConsumable
    {
        public ConsumableEffect(float duration) : base(duration) { }

        public event Action EarlyConsumed;

        public void Consume() => EarlyConsumed?.Invoke();
    }
}

