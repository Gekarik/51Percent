public interface IRespawnScheduler : IRespawnTracker
{
    bool TryScheduleRespawn(ICharacter character);
}
