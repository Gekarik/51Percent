using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneInteractionTests : GameplayScenario
{
    [UnityTest]
    public IEnumerator EnteringSpikedTrail_WithWings_EscapesWithoutLeavingTrailAtOldPosition()
    {
        yield return CrossSpikedTrail(false);
    }

    [UnityTest]
    public IEnumerator WideCaptureTouchingSpikedTrail_WithWings_StopsProcessingSurroundingCells()
    {
        yield return CrossSpikedTrail(true);
    }

    private IEnumerator CrossSpikedTrail(bool wideCapture)
    {
        var center = Hexes.First(hex => Read(hex, "Owner") == null && Neighbors(hex).Length == 6
            && Neighbors(hex).All(neighbor => Read(neighbor, "Owner") == null));
        var attacked = wideCapture ? Neighbors(center).First() : center;
        Call(Territory, "TrailHex", Enemy, attacked);
        Assert.That(Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f)), Is.EqualTo(true));
        Assert.That(Call(Enemy, "TryAcceptBooster", New("SpikesBoosterEffect", 10f, null)), Is.EqualTo(true));
        Call(Read(Player, "Stats"), "SetBase", Enum.Parse(GameType("StatType"), "CaptureWidth"), wideCapture ? 2f : 1f);

        var conqueror = (Behaviour)Part(Player, "Conqueror");
        conqueror.enabled = true;
        Call(Part(Player, "Mover"), "TeleportTo", center.transform.position + Vector3.up * 0.1f);
        Time.timeScale = 1f;
        yield return new WaitForFixedUpdate();
        yield return null;
        Time.timeScale = 0f;
        conqueror.enabled = false;

        Assert.That(Alive(Player) && Alive(Enemy), Is.True);
        Assert.That(Read(Read(Player, "Boosters"), "HasActiveBooster"), Is.EqualTo(false));
        Assert.That(Read(Read(Enemy, "Boosters"), "HasActiveBooster"), Is.EqualTo(false));
        Assert.That(((IEnumerable)Read(conqueror, "TrailHexes")).Cast<object>(), Is.Empty);
        Assert.That(Read(attacked, "Owner"), Is.SameAs(Enemy));
        Assert.That(Neighbors(center).Append(center).Any(hex => ReferenceEquals(Read(hex, "Owner"), Player)), Is.False);
        var refuge = Call(Grid, "GetHexAt", Player.transform.position);
        Assert.That(Read(refuge, "Owner"), Is.SameAs(Player));
        Assert.That(Kills(Player) + Kills(Enemy), Is.Zero);
    }

    [UnityTest]
    public IEnumerator CoinTrigger_AddsOneCoinToModelAndHud_ThenReturnsCoinToPool()
    {
        int before = (int)Read(Read(Player, "LifeStats"), "Coins");
        var coin = (Component)Call(Find("CoinSpawner").Single(), "SpawnTyped",
            Player.transform.position + Vector3.right * 5f);
        ((Behaviour)Part(Player, "Grabber")).enabled = true;
        Time.timeScale = 1f;
        yield return new WaitForFixedUpdate();
        Call(Part(Player, "Mover"), "TeleportTo", coin.transform.position);
        float deadline = Time.realtimeSinceStartup + 3f;
        while (coin.gameObject.activeSelf && Time.realtimeSinceStartup < deadline)
            yield return null;
        Time.timeScale = 0f;

        Assert.That((int)Read(Read(Player, "LifeStats"), "Coins"), Is.EqualTo(before + 1));
        Assert.That(coin.gameObject.activeSelf, Is.False);
        var view = Find("PlayerStatsView").Single();
        Assert.That(Read(Field(view, "_coinsText"), "text"), Is.EqualTo($"Coins: {before + 1}"));
    }

    [UnityTest]
    public IEnumerator RestartAfterFinish_CreatesRunningMatchAndFreshPlayer()
    {
        Call(Player, "AcceptCoin");
        Call(WinConditions, "ForceFinish", Enemy);
        Assert.That(Time.timeScale, Is.Zero);
        Call(Find("GameManager").Single(), "Restart");
        yield return null;
        var newPlayer = Find("Player").Single();
        var newMatch = Field(Find("Bootstrap").Single(), "_matchState");
        Assert.That(newPlayer, Is.Not.SameAs(Player));
        Assert.That(newMatch, Is.Not.SameAs(Match));
        Assert.That(Read(newMatch, "IsRunning"), Is.EqualTo(true));
        Assert.That(Time.timeScale, Is.EqualTo(1f));
        Assert.That((int)Read(Read(newPlayer, "LifeStats"), "Coins"), Is.Zero);
        Assert.That(Kills(newPlayer), Is.Zero);
        FreezeWorld();
        Time.timeScale = 0f;
    }
}
