using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "51_Percent/Booster Icon Registry")]
public class BoosterIconRegistry : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        [SerializeField] private BoosterId _id;
        [SerializeField] private Sprite _icon;

        public BoosterId Id => _id;
        public Sprite Icon => _icon;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    public Sprite Get(BoosterId id)
    {
        foreach (var entry in _entries)
        {
            if (entry.Id == id)
                return entry.Icon;
        }

        return null;
    }
}
