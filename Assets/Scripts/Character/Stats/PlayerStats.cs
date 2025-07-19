using System;

public class PlayerStats
{
    public event Action<int> CoinsChanged;
    public event Action<int> KillsChanged;

    public int Coins { get; private set; }
    public int Kills { get; private set; }

    public void AddCoin()
    {
        Coins++;
        CoinsChanged?.Invoke(Coins);
    }

    public void AddKill()
    {
        Kills++;
        KillsChanged?.Invoke(Kills);
    }
}