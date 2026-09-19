using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Проп активного бустера: доска под ногами, крылья за спиной. Экземпляр создаётся
// лениво, по одному на бустер и на персонажа, дальше только переключается активность.
// Владелец видимости — этот компонент, событие домена лишь вход: поэтому исход
// крыльев спокойно переживает закрытие самого эффекта
public class BoosterPropAttacher : MonoBehaviour
{
    [SerializeField] private BoosterVisualRegistry _registry;

    private readonly Dictionary<BoosterId, GameObject> _props = new Dictionary<BoosterId, GameObject>();
    private readonly Dictionary<BoosterId, Vector3> _baseScales = new Dictionary<BoosterId, Vector3>();

    private IBoosterLifecycle _lifecycle;
    private ICharacter _character;
    private GameObject _visible;
    private Tween _outro;

    public void Init(IBoosterLifecycle lifecycle, ICharacter character)
    {
        if (_registry == null)
            throw new InvalidOperationException($"[{GetType().Name}] _registry не назначен на '{name}'");

        _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        _character = character ?? throw new ArgumentNullException(nameof(character));

        _lifecycle.BoosterStarted += OnBoosterStarted;
        _lifecycle.BoosterEnded += OnBoosterEnded;
    }

    private void OnDestroy()
    {
        if (_lifecycle != null)
        {
            _lifecycle.BoosterStarted -= OnBoosterStarted;
            _lifecycle.BoosterEnded -= OnBoosterEnded;
        }

        DOTween.Kill(this);
    }

    // Побеждает свежее событие: незавершённый исход прошлого пропа обрывается сразу,
    // иначе он догорел бы поверх уже надетого нового
    private void OnBoosterStarted(BoosterId id)
    {
        HideImmediately();

        if (!_registry.TryGetProp(id, out BoosterPropPresentation presentation))
            return;

        _visible = Resolve(id, presentation);
        _visible.transform.localScale = _baseScales[id];
        _visible.SetActive(true);
    }

    private void OnBoosterEnded(BoosterId id, BoosterEndReason reason)
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

    private void PlayOutro(GameObject prop, BoosterPropPresentation presentation)
    {
        _visible = null;
        _outro = prop.transform
            .DOScale(Vector3.zero, presentation.OutroDuration)
            .SetEase(presentation.OutroEase)
            .SetLink(gameObject)
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
        // worldPositionStays: false — локальная поза префаба становится локальной в сокете
        var created = Instantiate(presentation.Prefab, socket, false);
        created.SetActive(false);

        _props[id] = created;
        _baseScales[id] = created.transform.localScale;
        return created;
    }
}
