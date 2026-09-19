public class MushroomBoosterEffect : IBoosterEffect
{
    private readonly StatModifier _captureWidthModifier;
    private readonly StatModifier _speedModifier;

    public BoosterId BoosterId => BoosterId.Mushroom;
    public float Duration { get; }

    public MushroomBoosterEffect(float duration, float speedReductionFactor, float captureWidthBonus)
    {
        Duration = duration;
        _captureWidthModifier = new StatModifier(captureWidthBonus, ModifierType.Flat);
        _speedModifier = new StatModifier(-speedReductionFactor, ModifierType.Percent);
    }

    public void Apply(IBoosterContext context)
    {
        context.Stats.AddModifier(StatType.CaptureWidth, _captureWidthModifier);
        context.Stats.AddModifier(StatType.Speed, _speedModifier);
    }

    public void Remove(IBoosterContext context)
    {
        context.Stats.RemoveModifier(StatType.CaptureWidth, _captureWidthModifier);
        context.Stats.RemoveModifier(StatType.Speed, _speedModifier);
    }
}
