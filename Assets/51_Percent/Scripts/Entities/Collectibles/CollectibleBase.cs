using System;
using UnityEngine;

public abstract class CollectibleBase : MonoBehaviour, ICollectible
{
    [SerializeField] private CollectibleViewBase _viewBase;
    
    private IGrabbableView _viewInterface;

    public event Action<ICollectible> Collected;

    public GrabbableState State { get; protected set; }
    public Transform Transform => transform;

    protected virtual void Awake()
    {
        State = GrabbableState.Idle;
        _viewInterface = _viewBase as IGrabbableView;
    }

    private void OnEnable()
    {
        _viewInterface.AnimationCompleted += OnViewAnimationCompleted;
    }

    private void OnDisable()
    {
        _viewInterface.AnimationCompleted -= OnViewAnimationCompleted;
    }

    public void Collect()
    {
        State = GrabbableState.Collected;
        _viewInterface.PlayCollectAnimation();
    }

    private void OnViewAnimationCompleted()
    {
        State = GrabbableState.Idle;
        _viewInterface.ResetViewState();
        Collected?.Invoke(this);
    }
}