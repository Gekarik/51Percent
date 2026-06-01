public class SpeedBoosterEffect : IBoosterEffect
{
    private readonly float _duration;
    private readonly StatModifier _modifier;

    public string BoosterId => "speed";
    public float Duration => _duration;
    public SpeedBoosterEffect(float duration, float speedBonus)
    {
        _duration = duration;
        _modifier = new StatModifier(speedBonus, ModifierType.Percent);
    }

    public void Apply(IBoosterContext context) =>
        context.Stats.AddModifier(StatType.Speed, _modifier);

    public void Remove(IBoosterContext context) =>
        context.Stats.RemoveModifier(StatType.Speed, _modifier);
}
