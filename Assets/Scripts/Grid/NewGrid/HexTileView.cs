using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(HexTile))]
public class HexTileView : MonoBehaviour, IHexAnimationHandler
{
    [Header("References")]
    [Tooltip("Только визуальный слой, двигаем его, а не сам объект с коллайдером")]
    [SerializeField] private Transform visual;

    [Header("Settings")]
    [SerializeField] private AnimationSettingsSO settings;
    [SerializeField] private Gradient ownerColorGradient;

    private HexTile _model;
    private Vector3 _initialLocalPos;
    private Vector3 _initialScale;
    private Tween _currentTween;

    private void Awake()
    {
        if (visual == null)
        {
            Debug.LogError("Visual Transform is not assigned!", this);
            enabled = false;
            return;
        }

        _initialLocalPos = visual.localPosition;
        _initialScale = visual.localScale;

        _model = GetComponent<HexTile>();
        _model.OnOwnerChanged += HandleOwnerChanged;
    }

    private void OnDestroy()
    {
        _model.OnOwnerChanged -= HandleOwnerChanged;
        Stop();
    }

    private void HandleOwnerChanged(int newOwnerId, UnityEngine.Color newColor)
    {
        var mr = visual.GetComponent<MeshRenderer>();

        if (mr != null)
            mr.material.color = newColor;

        ResetPositionAndScale();
        Stretch();
    }

    public Tween Bounce()
    {
        StopCurrentTween();
        visual.localPosition = _initialLocalPos;

        _currentTween = visual
            .DOLocalJump(_initialLocalPos + Vector3.up * settings.bounceHeight,
                         settings.bounceHeight,
                         settings.bounceJumps,
                         settings.bounceDuration)
            .SetEase(settings.bounceEase)
            .OnComplete(() => visual.localPosition = _initialLocalPos);

        return _currentTween;
    }

    public Tween Stretch()
    {
        StopCurrentTween();
        visual.localScale = _initialScale;

        var seq = DOTween.Sequence();
        seq.Append(visual.DOScale(settings.stretchScale, settings.stretchDuration)
                      .SetEase(settings.stretchEaseUp));
        seq.Append(visual.DOScale(_initialScale, settings.stretchDuration)
                      .SetEase(settings.stretchEaseDown));

        _currentTween = seq;
        return seq;
    }

    public void Stop()
    {
        StopCurrentTween();
        ResetPositionAndScale();
    }

    private void StopCurrentTween()
    {
        if (_currentTween != null && _currentTween.IsActive())
        {
            _currentTween.Kill();
            _currentTween = null;
        }
    }

    private void ResetPositionAndScale()
    {
        visual.localPosition = _initialLocalPos;
        visual.localScale = _initialScale;
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }
}
