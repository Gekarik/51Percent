using System;
using UnityEngine;

public abstract class CollectibleBase : MonoBehaviour, ICollectible
{
    [Required] [SerializeField] private CollectibleViewBase _view;

    private ICollectibleView _viewInterface;

    public event Action<ICollectible> Consumed;
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

    // Логический результат наступает сразу: предмет выбывает с поля в момент подбора,
    // а не когда доиграет анимация. Иначе подобранный предмет ещё числился бы на карте
    public void Collect()
    {
        if (State != CollectibleState.Idle)
            return;

        State = CollectibleState.Collected;
        Consumed?.Invoke(this);
        _viewInterface.PlayCollectAnimation();
    }

    public abstract bool TryApplyTo(ICollectibleConsumer consumer);

    private void OnViewAnimationCompleted()
    {
        _viewInterface.ResetViewState();
        Collected?.Invoke(this);
    }
}