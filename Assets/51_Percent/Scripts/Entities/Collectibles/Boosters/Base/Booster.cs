public abstract class Booster : CollectibleBase
{
    public abstract IBoosterEffect CreateEffect();

    public override bool TryApplyTo(ICollectibleConsumer consumer) => consumer.TryAcceptBooster(CreateEffect());
}
