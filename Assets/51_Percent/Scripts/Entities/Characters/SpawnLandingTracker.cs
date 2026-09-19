using System;
using UnityEngine;

// Окно приземления при появлении персонажа: тело всё время стоит на своей точке,
// но персонаж не участвует в игре, пока окно открыто. Домену принадлежит только
// факт и срок окна — как выглядит спуск, решает презентация
public class SpawnLandingTracker : MonoBehaviour
{
    private const float MinDuration = 0.01f;
    private const float DefaultDuration = 0.75f;

    [SerializeField, Min(MinDuration)] private float _duration = DefaultDuration;

    private float _elapsed;

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

    private void Update()
    {
        if (!IsLanding)
            return;

        _elapsed += Time.deltaTime;

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
