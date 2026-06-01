using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "51_Percent/Booster Icon Registry")]
public class BoosterIconRegistry : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public string BoosterId;
        public Sprite Icon;
    }

    [SerializeField] private List<Entry> _entries;

    public Sprite Get(string boosterId)
    {
        foreach (var entry in _entries)
        {
            if (entry.BoosterId == boosterId)
                return entry.Icon;
        }

        return null;
    }
}
