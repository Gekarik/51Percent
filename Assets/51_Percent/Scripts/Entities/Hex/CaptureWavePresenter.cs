using System.Collections.Generic;
using UnityEngine;

// Презентация захвата: превращает доменное событие «захвачены гексы» в волновую анимацию вьюшек
public class CaptureWavePresenter
{
    private readonly IWaveAnimator _waveAnimator;
    private readonly List<Transform> _viewsBuffer = new List<Transform>();

    public CaptureWavePresenter(IWaveAnimator waveAnimator)
    {
        _waveAnimator = waveAnimator;
    }

    public void OnAreaCaptured(ICharacter owner, IReadOnlyList<IHex> hexes)
    {
        _viewsBuffer.Clear();

        foreach (var hex in hexes)
        {
            if (hex.ViewTransform != null && !_viewsBuffer.Contains(hex.ViewTransform))
                _viewsBuffer.Add(hex.ViewTransform);
        }

        _waveAnimator.Wave(_viewsBuffer, owner.Transform.position);
    }
}
