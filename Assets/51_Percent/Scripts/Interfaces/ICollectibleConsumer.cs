// Приёмник подобранных предметов: коллектибл сам выбирает нужный метод (двойная диспетчеризация),
// а персонаж решает, брать ли предмет и что с ним делать
public interface ICollectibleConsumer
{
    bool CanAcceptBooster { get; }
    void AcceptCoin();
    bool TryAcceptBooster(IBoosterEffect effect);
}
