using System.Collections.Generic;

public class CharacterStats
{
    private readonly Dictionary<StatType, float> _baseValues = new Dictionary<StatType, float>();
    private readonly Dictionary<StatType, List<StatModifier>> _modifiers = new Dictionary<StatType, List<StatModifier>>();

    public void SetBase(StatType stat, float value)
    {
        _baseValues[stat] = value;
    }

    public float GetValue(StatType stat)
    {
        if (!_baseValues.TryGetValue(stat, out float baseValue))
            return 0f;

        if (!_modifiers.TryGetValue(stat, out var mods) || mods.Count == 0)
            return baseValue;

        float flat = 0f;
        float percent = 1f;

        foreach (var mod in mods)
        {
            if (mod.Type == ModifierType.Flat)
                flat += mod.Value;
            else
                percent += mod.Value;
        }

        return (baseValue + flat) * percent;
    }

    public void AddModifier(StatType stat, StatModifier modifier)
    {
        if (!_modifiers.ContainsKey(stat))
            _modifiers[stat] = new List<StatModifier>();

        _modifiers[stat].Add(modifier);
    }

    public void RemoveModifier(StatType stat, StatModifier modifier)
    {
        if (_modifiers.TryGetValue(stat, out var mods))
            mods.Remove(modifier);
    }
}
