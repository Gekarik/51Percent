public interface ICollectibleConsumer
{
    bool CanAcceptBooster { get; }
    void AcceptCoin();
    bool TryAcceptBooster(IBoosterEffect effect);
}
