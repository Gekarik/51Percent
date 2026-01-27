using System.Collections;
using UnityEngine;

public class Colorizer
{
    private const string ColorProperty = "_Color";

    private readonly Renderer _renderer;
    private readonly MaterialPropertyBlock _materialPropertyBlock;
    private readonly HexViewSettings _settings;
    private readonly Color _defaultColor;
    private readonly ICoroutineRunner _runner;

    private Coroutine _runningCoroutine;

    public Colorizer(Renderer renderer, ICoroutineRunner runner, HexViewSettings settings)
    {
        _renderer = renderer;
        _runner = runner;
        _settings = settings;

        _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _defaultColor = _renderer.sharedMaterial.GetColor(ColorProperty);
    }

    public void CancelTransition()
    {
        if (_runningCoroutine != null)
        {
            _runner.StopRoutine(_runningCoroutine);
            _runningCoroutine = null;
        }
    }

    public void ResetColor()
    {
        TransitionTo(_defaultColor, _settings.ResetColorDuration);
    }

    public void SetColorSlowly(Color color)
    {
        TransitionTo(color, _settings.ColorizeDuration, Color.black);
    }

    public void SetColorInstantly(Color color)
    {
        CancelTransition();
        ApplyColor(color);
    }

    private void TransitionTo(Color targetColor, float duration, Color? overrideStartColor = null)
    {
        CancelTransition();
        _runningCoroutine = _runner.StartRoutine(ColorTransitionRoutine(targetColor, duration, overrideStartColor));
    }

    private IEnumerator ColorTransitionRoutine(Color targetColor, float duration, Color? overrideStartColor)
    {
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        Color startColor = overrideStartColor ?? _materialPropertyBlock.GetColor(ColorProperty);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyColor(Color.Lerp(startColor, targetColor, t));
            yield return null;
        }

        ApplyColor(targetColor);
        _runningCoroutine = null;
    }

    private void ApplyColor(Color color)
    {
        _materialPropertyBlock.SetColor(ColorProperty, color);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
