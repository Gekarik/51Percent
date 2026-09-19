// Знает, вернётся ли выбывший персонаж в игру — правила конца игры сверяются с этим
public interface IRespawnTracker
{
    bool HasPendingRespawn { get; }
}
