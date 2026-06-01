public interface IBoosterEffect
{
    string BoosterId { get; }
    float Duration { get; }
    void Apply(IBoosterContext context);
    void Remove(IBoosterContext context);
}
