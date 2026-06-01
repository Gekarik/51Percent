using System;
using UnityEngine;

public abstract class CollectibleBase : MonoBehaviour, ICollectible
{
    [SerializeField] private CollectibleViewBase _view;

    private ICollectibleView _viewInterface;

    public event Action<ICollectible> Collected;

    public CollectibleState State { get; protected set; }
    public Transform Transform => transform;

    private void Awake()
    {
        if (_view == null)
            throw new InvalidOperationException($"[{GetType().Name}] _view не назначен на '{name}'");

        _viewInterface = _view as ICollectibleView;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_view == null)
            Debug.LogError($"[{GetType().Name}] _view не назначен на '{name}'", this);
    }
#endif

    private void OnEnable()
    {
        State = CollectibleState.Idle;
        _viewInterface.AnimationCompleted += OnViewAnimationCompleted;
    }

    private void OnDisable()
    {
        _viewInterface.AnimationCompleted -= OnViewAnimationCompleted;
    }

    public void Collect()
    {
        State = CollectibleState.Collected;
        _viewInterface.PlayCollectAnimation();
    }

    private void OnViewAnimationCompleted()
    {
        _viewInterface.ResetViewState();
        Collected?.Invoke(this);
    }
}