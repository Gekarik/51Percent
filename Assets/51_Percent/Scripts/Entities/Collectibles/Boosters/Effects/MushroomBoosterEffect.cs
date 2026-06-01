public class MushroomBoosterEffect : IBoosterEffect
{
    private readonly StatModifier _captureWidthModifier;
    private readonly StatModifier _speedModifier;
    private readonly float _modelScaleFactor;

    public string BoosterId => "mushroom";
    public float Duration { get; }
    public MushroomBoosterEffect(float duration, float speedReductionFactor, float modelScaleFactor, float captureWidthBonus)
    {
        Duration = duration;
        _captureWidthModifier = new StatModifier(captureWidthBonus, ModifierType.Flat);
        _speedModifier = new StatModifier(-speedReductionFactor, ModifierType.Percent);
        _modelScaleFactor = modelScaleFactor;
    }

    public void Apply(IBoosterContext context)
    {
        context.Stats.AddModifier(StatType.CaptureWidth, _captureWidthModifier);
        context.Stats.AddModifier(StatType.Speed, _speedModifier);
        context.SetModelScale(_modelScaleFactor);
    }

    public void Remove(IBoosterContext context)
    {
        context.Stats.RemoveModifier(StatType.CaptureWidth, _captureWidthModifier);
        context.Stats.RemoveModifier(StatType.Speed, _speedModifier);
        context.SetModelScale(1f);
    }
}
