using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoosterPropView
{
    private readonly BoosterVisualRegistry _registry;
    private readonly ICharacter _character;
    private readonly GameObject _lifetimeOwner;
    private readonly Dictionary<BoosterId, GameObject> _props = new Dictionary<BoosterId, GameObject>();
    private readonly Dictionary<BoosterId, Vector3> _baseScales = new Dictionary<BoosterId, Vector3>();

    private GameObject _visible;
    private Tween _outro;

    public BoosterPropView(BoosterVisualRegistry registry, ICharacter character, GameObject lifetimeOwner)
    {
        _registry = registry;
        _character = character;
        _lifetimeOwner = lifetimeOwner;
    }

    public void Show(BoosterId id)
    {
        HideImmediately();

        if (!_registry.TryGetProp(id, out BoosterPropPresentation presentation))
            return;

        _visible = Resolve(id, presentation);
        _visible.transform.localScale = _baseScales[id];
        _visible.SetActive(true);
    }

    public void Hide(BoosterId id, BoosterEndReason reason)
    {
        if (_visible == null)
            return;

        if (reason != BoosterEndReason.Consumed || !_registry.TryGetProp(id, out BoosterPropPresentation presentation)
            || presentation.OutroDuration <= 0f)
        {
            HideImmediately();
            return;
        }

        PlayOutro(_visible, presentation);
    }

    public void Dispose()
    {
        DOTween.Kill(this);
    }

    private void PlayOutro(GameObject prop, BoosterPropPresentation presentation)
    {
        _visible = null;
        _outro = prop.transform
            .DOScale(Vector3.zero, presentation.OutroDuration)
            .SetEase(presentation.OutroEase)
            .SetLink(_lifetimeOwner)
            .SetTarget(this)
            .OnComplete(() => prop.SetActive(false));
    }

    private void HideImmediately()
    {
        _outro?.Kill();
        _outro = null;

        if (_visible == null)
            return;

        _visible.SetActive(false);
        _visible = null;
    }

    private GameObject Resolve(BoosterId id, BoosterPropPresentation presentation)
    {
        if (_props.TryGetValue(id, out GameObject existing))
            return existing;

        var socket = _character.GetSocket(presentation.Socket);
        var created = Object.Instantiate(presentation.Prefab, socket, false);
        created.SetActive(false);

        _props[id] = created;
        _baseScales[id] = created.transform.localScale;
        return created;
    }
}
