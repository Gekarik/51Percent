using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Внешний вид бустеров в мире: походка, проп в сокете и размер модели.
// Одна запись на бустер, потому что при заведении нового всё это настраивается разом.
// Иконки живут отдельно: они в слое UI и у них другой потребитель
[CreateAssetMenu(fileName = "BoosterVisuals", menuName = "51_Percent/Booster Visual Registry")]
public class BoosterVisualRegistry : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        [SerializeField] private BoosterId _id;

        [Header("Походка")]
        [SerializeField] private LocomotionMode _mode;

        [Header("Проп")]
        [SerializeField] private GameObject _propPrefab;
        [SerializeField] private SocketType _socket;
        [SerializeField, Min(0f)] private float _propOutroDuration;
        [SerializeField] private Ease _propOutroEase;

        [Header("Размер модели")]
        [SerializeField, Min(0f)] private float _modelScale;
        [SerializeField, Min(0f)] private float _growDuration;
        [SerializeField, Min(0f)] private float _shrinkDuration;
        [SerializeField] private Ease _scaleEase;

        public BoosterId Id => _id;
        public LocomotionMode Mode => _mode;
        public bool HasProp => _propPrefab != null;
        public bool HasScale => _modelScale > 0f;

        public BoosterPropPresentation Prop =>
            new BoosterPropPresentation(_propPrefab, _socket, _propOutroDuration, _propOutroEase);

        public BoosterScalePresentation Scale =>
            new BoosterScalePresentation(_modelScale, _growDuration, _shrinkDuration, _scaleEase);
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    private Dictionary<BoosterId, Entry> _byId;

    // Список остаётся источником правды для инспектора, словарь строится из него:
    // поиск идёт на каждое событие бустера, линейный перебор тут лишний
    private void OnEnable() => Rebuild();

    private void OnValidate() => Rebuild();

    private void Rebuild()
    {
        _byId = new Dictionary<BoosterId, Entry>(_entries.Count);

        foreach (var entry in _entries)
            _byId[entry.Id] = entry;
    }

    public LocomotionMode GetMode(BoosterId id)
    {
        return TryGet(id, out Entry entry) ? entry.Mode : LocomotionMode.Default;
    }

    public bool TryGetProp(BoosterId id, out BoosterPropPresentation prop)
    {
        if (TryGet(id, out Entry entry) && entry.HasProp)
        {
            prop = entry.Prop;
            return true;
        }

        prop = default;
        return false;
    }

    public bool TryGetScale(BoosterId id, out BoosterScalePresentation scale)
    {
        if (TryGet(id, out Entry entry) && entry.HasScale)
        {
            scale = entry.Scale;
            return true;
        }

        scale = default;
        return false;
    }

    private bool TryGet(BoosterId id, out Entry entry)
    {
        if (_byId == null)
            Rebuild();

        return _byId.TryGetValue(id, out entry);
    }
}
