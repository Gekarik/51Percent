using System;
using UnityEngine;

// Презентационная реакция на смену бустера: переключает режим походки по идентификатору
// через реестр. Сам переход и смешивание живут в графе контроллера
public class BoosterAnimationSwitcher : MonoBehaviour
{
    [SerializeField] private BoosterVisualRegistry _registry;

    private IBoosterLifecycle _lifecycle;
    private CharacterView _view;

    public void Init(IBoosterLifecycle lifecycle, CharacterView view)
    {
        if (_registry == null)
            throw new InvalidOperationException($"[{GetType().Name}] _registry не назначен на '{name}'");

        _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        _view = view != null ? view : throw new ArgumentNullException(nameof(view));

        _lifecycle.BoosterStarted += OnBoosterStarted;
        _lifecycle.BoosterEnded += OnBoosterEnded;
    }

    private void OnDestroy()
    {
        if (_lifecycle == null)
            return;

        _lifecycle.BoosterStarted -= OnBoosterStarted;
        _lifecycle.BoosterEnded -= OnBoosterEnded;
    }

    private void OnBoosterStarted(BoosterId id)
    {
        _view.SetLocomotionMode(_registry.GetMode(id));
    }

    private void OnBoosterEnded(BoosterId id, BoosterEndReason reason)
    {
        _view.SetLocomotionMode(LocomotionMode.Default);
    }
}
