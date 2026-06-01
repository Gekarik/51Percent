using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoardView : MonoBehaviour
{
    [SerializeField] private LeaderBoardEntryView _entryPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private RectTransform _crown;
    [SerializeField] private BoosterIconRegistry _boosterIconRegistry;

    private LeaderBoardModel _model;
    private readonly List<LeaderBoardEntryView> _entryViews = new List<LeaderBoardEntryView>();
    private readonly Dictionary<ICharacter, LeaderBoardEntryView> _characterToView = new Dictionary<ICharacter, LeaderBoardEntryView>();
    private readonly Dictionary<ICharacter, Action> _boosterHandlers = new Dictionary<ICharacter, Action>();

    private void Awake()
    {
        if (_container == null)
            _container = transform;

        _crown = Instantiate(_crown);
    }

    public void Init(LeaderBoardModel model)
    {
        _model = model;
        _model.Changed += HandleModelChanged;
        HandleModelChanged();
    }

    private void OnEnable()
    {
        if (_model != null)
        {
            _model.Changed -= HandleModelChanged;
            _model.Changed += HandleModelChanged;
        }
    }

    private void OnDisable()
    {
        if (_model != null)
            _model.Changed -= HandleModelChanged;
    }

    private void HandleModelChanged()
    {
        SyncViews();
    }

    private void SyncViews()
    {
        var entries = _model.Entries;

        foreach (var entry in entries)
        {
            if (!_characterToView.ContainsKey(entry.Character))
                CreateEntryView(entry);
        }

        for (int i = _entryViews.Count - 1; i >= 0; i--)
        {
            var view = _entryViews[i];
            bool found = false;

            foreach (var entry in entries)
            {
                if (_characterToView.TryGetValue(entry.Character, out var mappedView) && mappedView == view)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                RemoveEntryView(view);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (_characterToView.TryGetValue(entry.Character, out var view))
            {
                view.SetData(i + 1, entry.Character.Name, entry.Percent, entry.Character.Color);
                view.transform.SetSiblingIndex(i);
            }
        }

        UpdateCrownParent(entries);
    }

    private void UpdateCrownParent(IReadOnlyList<LeaderBoardEntry> entries)
    {
        if (_crown == null || entries.Count == 0)
            return;

        if (_characterToView.TryGetValue(entries[0].Character, out var leaderView))
            _crown.SetParent(leaderView.transform, false);
    }

    private void CreateEntryView(LeaderBoardEntry entry)
    {
        var view = Instantiate(_entryPrefab, _container);
        _entryViews.Add(view);
        _characterToView[entry.Character] = view;

        var character = entry.Character;
        Action handler = () =>
        {
            if (!_characterToView.TryGetValue(character, out var entryView))
                return;

            var observable = character.BoosterObservable;
            var icon = observable.HasActiveBooster ? _boosterIconRegistry?.Get(observable.ActiveEffect.BoosterId) : null;
            entryView.SetBoosterIcon(icon);
        };

        _boosterHandlers[character] = handler;
        character.BoosterObservable.BoosterChanged += handler;
    }

    private void RemoveEntryView(LeaderBoardEntryView view)
    {
        ICharacter characterToRemove = null;

        foreach (var kvp in _characterToView)
        {
            if (kvp.Value == view)
            {
                characterToRemove = kvp.Key;
                break;
            }
        }

        if (characterToRemove != null)
        {
            _characterToView.Remove(characterToRemove);

            if (_boosterHandlers.TryGetValue(characterToRemove, out var handler))
            {
                characterToRemove.BoosterObservable.BoosterChanged -= handler;
                _boosterHandlers.Remove(characterToRemove);
            }
        }

        _entryViews.Remove(view);
        Destroy(view.gameObject);
    }
}
