using System.Collections;
using UnityEngine;

public class Colorizer
{
    private const string ColorProperty = "_Color";

    private readonly Renderer _renderer;
    private readonly MaterialPropertyBlock _materialPropertyBlock;
    private readonly HexViewSettings _settings;
    private readonly Color _defaultColor;

    private Coroutine _runningCoroutine;
    private readonly ICoroutineRunner _runner;

    public Colorizer(Renderer renderer, ICoroutineRunner runner, HexViewSettings settings)
    {
        _renderer = renderer;
        _runner = runner;
        _settings = settings;

        _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _defaultColor = _renderer.sharedMaterial.GetColor(ColorProperty);
    }

    public void CancelReset()
    {
        if (_runningCoroutine != null)
        {
            _runner.StopRoutine(_runningCoroutine);
            _runningCoroutine = null;
        }
    }

    public void ResetColor()
    {
        CancelReset();
        _runningCoroutine = _runner.StartRoutine(ResetColorRoutine());
    }

    public void SetColorSlowly(Color color)
    {
        CancelReset();
        _runningCoroutine = _runner.StartRoutine(SetColorRoutine(color));
    }

    public void SetColorInstantly(Color targetColor)
    {
        CancelReset();
        _materialPropertyBlock.SetColor(ColorProperty, targetColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private IEnumerator SetColorRoutine(Color targetColor)
    {
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        Color startColor = Color.black; //_materialPropertyBlock.GetColor(ColorProperty);
        float duration = _settings.ColorizeDuration;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            Color current = Color.Lerp(startColor, targetColor, time);
            _materialPropertyBlock.SetColor(ColorProperty, current);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }

        _materialPropertyBlock.SetColor(ColorProperty, targetColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
        _runningCoroutine = null;
    }

    private IEnumerator ResetColorRoutine()
    {
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        Color startColor = _materialPropertyBlock.GetColor(ColorProperty);
        float duration = _settings.ResetColorDuration;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            Color current = Color.Lerp(startColor, _defaultColor, time);
            _materialPropertyBlock.SetColor(ColorProperty, current);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }

        _materialPropertyBlock.SetColor(ColorProperty, _defaultColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
        _runningCoroutine = null;
    }
}
