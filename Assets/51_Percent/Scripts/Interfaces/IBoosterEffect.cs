public interface IBoosterEffect
{
    BoosterId BoosterId { get; }
    float Duration { get; }
    void Apply(IBoosterContext context);
    void Remove(IBoosterContext context);
}
