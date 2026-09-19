using UnityEditor;
using UnityEngine;

// Проигрывание клипа на объекте сцены без запуска игры: тем же механизмом,
// которым пользуется штатное окно Animation. Владеет только временем показа
public class AnimationPreviewer
{
    private GameObject _target;
    private AnimationClip _clip;
    private double _lastTick;

    public float Time { get; private set; }
    public bool IsPlaying { get; private set; }
    public static bool IsPreviewing => AnimationMode.InAnimationMode();

    public void Play(GameObject target, AnimationClip clip)
    {
        _target = target;
        _clip = clip;
        _lastTick = EditorApplication.timeSinceStartup;
        IsPlaying = true;
        Begin();
    }

    public void Pause() => IsPlaying = false;

    public void Stop()
    {
        IsPlaying = false;
        Time = 0f;

        if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
    }

    public void Scrub(GameObject target, AnimationClip clip, float time)
    {
        _target = target;
        _clip = clip;
        Time = time;
        Begin();
        Sample();
    }

    // Вызывается из окна каждый кадр редактора
    public void Tick()
    {
        if (!IsPlaying || _clip == null || _target == null)
            return;

        double now = EditorApplication.timeSinceStartup;
        Time += (float)(now - _lastTick);
        _lastTick = now;

        if (_clip.length > 0f)
            Time %= _clip.length;

        Sample();
    }

    private void Begin()
    {
        if (!AnimationMode.InAnimationMode())
            AnimationMode.StartAnimationMode();
    }

    private void Sample()
    {
        if (_clip == null || _target == null)
            return;

        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(_target, _clip, Time);
        AnimationMode.EndSampling();
    }
}
