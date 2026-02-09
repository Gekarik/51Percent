using System;
using System.Collections.Generic;

public class LeaderBoardModel
{
    private readonly TerritoryManager _territoryManager;
    private readonly List<LeaderBoardEntry> _entries = new List<LeaderBoardEntry>();

    public IReadOnlyList<LeaderBoardEntry> Entries => _entries;
    public event Action Changed;

    public LeaderBoardModel(TerritoryManager territoryManager)
    {
        _territoryManager = territoryManager;
        _territoryManager.OwnershipChanged += HandleOwnershipChanged;
    }

    public void Dispose()
    {
        _territoryManager.OwnershipChanged -= HandleOwnershipChanged;
    }

    public void RegisterCharacter(ICharacter character)
    {
        foreach (var entry in _entries)
        {
            if (entry.Character == character)
                return;
        }

        _entries.Add(new LeaderBoardEntry(character));
        RecalculateAndSort();
    }

    public void UnregisterCharacter(ICharacter character)
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (_entries[i].Character == character)
            {
                _entries.RemoveAt(i);
                break;
            }
        }

        Changed?.Invoke();
    }

    private void HandleOwnershipChanged()
    {
        RecalculateAndSort();
    }

    private void RecalculateAndSort()
    {
        foreach (var entry in _entries)
        {
            float percent = _territoryManager.GetOwnershipPercent(entry.Character);
            entry.UpdatePercent(percent);
        }

        _entries.Sort((a, b) => b.Percent.CompareTo(a.Percent));
        Changed?.Invoke();
    }
}
