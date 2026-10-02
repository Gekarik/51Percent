public class StatModifier
{
    public float Value { get; }
    public ModifierType Type { get; }

    public StatModifier(float value, ModifierType type)
    {
        Value = value;
        Type = type;
    }
}
