using DG.Tweening;
using System;
using UnityEngine;

public abstract class CollectibleViewBase : MonoBehaviour, IGrabbableView
{
    [SerializeField] private float _animationDuration = 0.1f;

    public event Action AnimationCompleted;

    private Sequence _currentSequence;
    private TransformSnapshot _initialTransformSnapshot;
    private Transform _viewTransform;

    private void Awake()
    {
        _viewTransform = transform;
        _initialTransformSnapshot = new TransformSnapshot(_viewTransform);
    }

    private void OnDisable()
    {
        _currentSequence?.Kill();
        _currentSequence = null;
    }

    public void PlayCollectAnimation()
    {
        _currentSequence = DOTween.Sequence();
        _currentSequence.Join(_viewTransform.DOScale(Vector3.zero, _animationDuration));

        _currentSequence.OnComplete(() => {
            AnimationCompleted?.Invoke();
            _currentSequence = null;
        });
    }

    public void ResetViewState()
    {
        _currentSequence?.Kill();
        _currentSequence = null;
        _initialTransformSnapshot.Restore(_viewTransform);
    }

    public void DisableView()
    {
        gameObject.SetActive(false);
    }
}