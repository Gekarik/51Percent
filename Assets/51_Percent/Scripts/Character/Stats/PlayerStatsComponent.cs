using UnityEngine;

public class PlayerStatsComponent : MonoBehaviour
{
    public PlayerStats Stats { get; private set; }

    void Awake()
    {
        Stats = new PlayerStats();
    }

    public void CollectCoin()
    {
        Stats.AddCoin();
    }

    public void RegisterKill()
    {
        Stats.AddKill();
    }
}