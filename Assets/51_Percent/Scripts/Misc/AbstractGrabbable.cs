using System;
using UnityEngine;

public abstract class AbstractGrabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] private AbstractGrabbableView _view;
    
    private IGrabbableView _viewInterface;

    public event Action<IGrabbable> Collected;

    public GrabbableState State { get; private set; }
    public Transform Transform => transform;

    protected virtual void Awake()
    {
        State = GrabbableState.Idle;
        _viewInterface = _view as IGrabbableView;
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