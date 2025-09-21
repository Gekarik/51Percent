using UnityEngine;

public interface IPlayableCharacter
{
    PlayerStatsComponent StatsComponent { get; }
    Camera Camera { get; }
}
