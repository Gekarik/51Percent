using System;
using UnityEngine;

public class BoosterPresentation : MonoBehaviour
{
    [Required] [SerializeField] private BoosterVisualRegistry _registry;

    private IBoosterLifecycle _lifecycle;
    private CharacterView _view;
    private BoosterPropView _prop;
    private BoosterScaleView _scale;

    public void Init(IBoosterLifecycle lifecycle, CharacterView view, ICharacter character)
    {
        if (_registry == null)
            throw new InvalidOperationException($"[{GetType().Name}] _registry не назначен на '{name}'");

        _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        _view = view != null ? view : throw new ArgumentNullException(nameof(view));

        if (character == null)
            throw new ArgumentNullException(nameof(character));

        _prop = new BoosterPropView(_registry, character, gameObject);
        _scale = new BoosterScaleView(_registry, _view, gameObject);

        _lifecycle.BoosterStarted += OnBoosterStarted;
        _lifecycle.BoosterEnded += OnBoosterEnded;
    }

    private void OnBoosterStarted(BoosterId id)
    {
        _view.SetLocomotionMode(_registry.GetMode(id));
        _prop.Show(id);
        _scale.Grow(id);
    }

    private void OnBoosterEnded(BoosterId id, BoosterEndReason reason)
    {
        _view.SetLocomotionMode(LocomotionMode.Default);
        _prop.Hide(id, reason);
        _scale.Restore(id);
    }

    private void OnDestroy()
    {
        if (_lifecycle != null)
        {
            _lifecycle.BoosterStarted -= OnBoosterStarted;
            _lifecycle.BoosterEnded -= OnBoosterEnded;
        }

        _prop?.Dispose();
        _scale?.Dispose();
    }
}
