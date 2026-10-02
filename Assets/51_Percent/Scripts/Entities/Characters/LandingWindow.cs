using System;
using UnityEngine;

// Окно приземления при появлении персонажа: тело всё время стоит на своей точке,
// но персонаж не участвует в игре, пока окно открыто. Домену принадлежит только
// факт и срок окна — как выглядит спуск, решает презентация.
// Обычный объект: время подаёт владелец, поэтому на паузе окно не истекает
public class LandingWindow
{
    private const float MinDuration = 0.01f;

    private readonly float _duration;

    private float _elapsed;

    public LandingWindow(float duration)
    {
        _duration = Mathf.Max(MinDuration, duration);
    }

    public bool IsLanding { get; private set; }
    public float Progress => Mathf.Clamp01(_elapsed / _duration);

    public event Action Started;
    public event Action Finished;

    public void Begin()
    {
        _elapsed = 0f;
        IsLanding = true;
        Started?.Invoke();
    }

    // Прерывание окна закрывает его так же, как естественное истечение:
    // подписчикам в обоих случаях нужно вернуть персонажа в обычное состояние
    public void Cancel()
    {
        if (IsLanding)
            Close();
    }

    public void Tick(float deltaTime)
    {
        if (!IsLanding || deltaTime <= 0f)
            return;

        _elapsed += deltaTime;

        if (_elapsed >= _duration)
            Close();
    }

    private void Close()
    {
        _elapsed = _duration;
        IsLanding = false;
        Finished?.Invoke();
    }
}
