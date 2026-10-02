using System;
using UnityEngine;

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
