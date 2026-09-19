using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class CollectibleViewBase : MonoBehaviour, ICollectibleView
{
    [SerializeField] private CollectibleViewSettings _settings;

    public event Action AnimationCompleted;

    private Sequence _collectSequence;
    private Tween _bobTween;
    private Tween _rotationTween;
    private TransformSnapshot _initialTransformSnapshot;
    private Transform _viewTransform;

    private void Awake()
    {
        if (_settings == null)
            throw new InvalidOperationException($"[{GetType().Name}] _settings не назначен на '{name}'");

        _viewTransform = transform;
        AlignMeshBottomToPivot();
        _initialTransformSnapshot = new TransformSnapshot(_viewTransform);
    }

    // Низ меша садится на origin, чтобы спавн по высоте не зависел от пивота конкретного префаба
    private void AlignMeshBottomToPivot()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Vector3 localPosition = _viewTransform.localPosition;
        localPosition.y = -meshFilter.sharedMesh.bounds.min.y * _viewTransform.localScale.y;
        _viewTransform.localPosition = localPosition;
    }

    private void OnEnable()
    {
        _initialTransformSnapshot.Restore(_viewTransform);
        StartIdleAnimation();
    }

    private void OnDisable()
    {
        KillIdleAnimation();
        _collectSequence?.Kill();
        _collectSequence = null;
    }

    public void PlayCollectAnimation()
    {
        KillIdleAnimation();

        _collectSequence = DOTween.Sequence();
        _collectSequence.Join(_viewTransform.DOScale(Vector3.zero, _settings.CollectDuration));
        _collectSequence.OnComplete(() => {
            AnimationCompleted?.Invoke();
            _collectSequence = null;
        });
    }

    public void ResetViewState()
    {
        _collectSequence?.Kill();
        _collectSequence = null;
    }

    public void DisableView()
    {
        gameObject.SetActive(false);
    }

    private void StartIdleAnimation()
    {
        // Локальная анимация: позиция спавна выставляется после OnEnable, мировой Y тут ещё неактуален
        float startY = _viewTransform.localPosition.y;

        _bobTween = _viewTransform
            .DOLocalMoveY(startY + _settings.BobHeight, _settings.BobDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(_settings.BobEase);

        float fullCycle = _settings.BobDuration * 2f;
        float phaseOffset = Random.Range(0f, fullCycle) * _settings.PhaseRandomness;
        if (phaseOffset > 0f)
            _bobTween.Goto(phaseOffset, andPlay: true);

        Quaternion initialRotation = _viewTransform.rotation;
        _rotationTween = DOVirtual.Float(0f, 360f, _settings.RotationDuration,
            angle => _viewTransform.rotation = Quaternion.AngleAxis(angle, Vector3.up) * initialRotation)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    private void KillIdleAnimation()
    {
        _bobTween?.Kill();
        _rotationTween?.Kill();
        _bobTween = null;
        _rotationTween = null;
    }
}
