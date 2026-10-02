using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MatchRulesTests : GameplayScenario
{
    [UnityTest]
    public IEnumerator BoosterCommands_RespectPauseLandingAndFinishedMatch()
    {
        var handler = Read(Player, "Boosters");
        var effect = New("SpeedBoosterEffect", 10f, 50f);

        // Подобранный бустер применяется сразу: промежуточного состояния у бустера нет
        Assert.That(Call(Player, "TryAcceptBooster", effect), Is.EqualTo(true));
        Assert.That(Read(handler, "ActiveEffect"), Is.SameAs(effect));
        Assert.That(Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f)), Is.EqualTo(false),
            "Второй бустер принят поверх действующего.");
        Call(handler, "Clear");

        Call(Match, "TryPause");
        Assert.That(Time.timeScale, Is.Zero);
        Assert.That(Call(Player, "TryAcceptBooster", effect), Is.EqualTo(false), "Бустер применён на паузе.");

        Call(Match, "TryResume");
        Time.timeScale = 0f;
        Call(Field(Player, "_landing"), "Begin");
        Assert.That(Call(Player, "TryAcceptBooster", effect), Is.EqualTo(false),
            "Бустер применён во время приземления.");
        Call(Field(Player, "_landing"), "Cancel");
        Assert.That(Call(Player, "TryAcceptBooster", effect), Is.EqualTo(true));
        Call(handler, "Clear");

        Call(WinConditions, "ForceFinish", Enemy);
        Assert.That(Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f)), Is.EqualTo(false),
            "Бустер применён после конца матча.");
        Assert.That(Call(Match, "TryResume"), Is.EqualTo(false));
        Assert.That(Time.timeScale, Is.Zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ActiveEffect_PausesItsTimer_ThenExpiresAndRestoresSpeed()
    {
        var handler = Read(Player, "Boosters");
        var stats = Read(Player, "Stats");
        var speed = Enum.Parse(GameType("StatType"), "Speed");
        float original = (float)Call(stats, "GetValue", speed);
        Call(Player, "TryAcceptBooster", New("SpeedBoosterEffect", 0.1f, 50f));
        Assert.That((float)Call(stats, "GetValue", speed), Is.GreaterThan(original));
        Call(Match, "TryPause");
        float remaining = (float)Read(handler, "RemainingTime");
        yield return new WaitForSecondsRealtime(0.2f);
        Assert.That(Read(handler, "HasActiveBooster"), Is.EqualTo(true));
        Assert.That((float)Read(handler, "RemainingTime"), Is.EqualTo(remaining).Within(0.001f));
        Call(Match, "TryResume");
        float deadline = Time.realtimeSinceStartup + 3f;
        while ((bool)Read(handler, "HasActiveBooster") && Time.realtimeSinceStartup < deadline)
        {
            FreezeWorld();
            yield return null;
        }
        Time.timeScale = 0f;
        Assert.That(Read(handler, "HasActiveBooster"), Is.EqualTo(false));
        Assert.That((float)Call(stats, "GetValue", speed), Is.EqualTo(original).Within(0.001f));
    }

    [UnityTest]
    public IEnumerator RepeatedDeath_ClearsActiveBooster_AndDoesNotRepeatRewardsOrRespawn()
    {
        var handler = Read(Player, "Boosters");
        Call(Player, "TryAcceptBooster", New("SpeedBoosterEffect", 10f, 50f));
        Call(Player, "AcceptCoin");
        int coins = Find("Coin").Length;
        int notifications = 0;
        ObserveResult(Combat, "CharacterEliminated", victim =>
        {
            notifications++;
            Assert.That(victim, Is.SameAs(Player));
            Assert.That(OwnedCount(Player), Is.Zero);
            Assert.That(Read(Find("PlayerRespawner").Single(), "HasPendingRespawn"), Is.EqualTo(true));
            Assert.That(Find("Coin").Length, Is.EqualTo(coins + 1));
        });
        Call(Combat, "OnTrailInterrupted", Player, Enemy);
        Assert.That(Read(handler, "HasActiveBooster"), Is.EqualTo(false));
        Assert.That(Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f)), Is.EqualTo(false));
        Call(Combat, "OnTrailInterrupted", Player, Enemy);
        Call(Combat, "OnTrailOrphaned", Player);
        Assert.That(Call(Player, "TryDie"), Is.EqualTo(false));
        Assert.That(Kills(Enemy), Is.EqualTo(1));
        Assert.That(notifications, Is.EqualTo(1));
        Assert.That(Read(Match, "IsFinished"), Is.EqualTo(false));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Death_ClearsActiveModifier_AndFinalMatchCancelsPendingRespawn()
    {
        var handler = Read(Player, "Boosters");
        var stats = Read(Player, "Stats");
        var speed = Enum.Parse(GameType("StatType"), "Speed");
        float original = (float)Call(stats, "GetValue", speed);
        Call(Player, "TryAcceptBooster", New("SpeedBoosterEffect", 10f, 50f));
        Call(Combat, "OnTrailInterrupted", Player, Enemy);
        Assert.That(Read(handler, "HasActiveBooster"), Is.EqualTo(false));
        Assert.That((float)Call(stats, "GetValue", speed), Is.EqualTo(original).Within(0.001f));
        Call(WinConditions, "ForceFinish", Enemy);
        Assert.That(Read(Find("PlayerRespawner").Single(), "HasPendingRespawn"), Is.EqualTo(false));
        // Даже если внешняя система снова пустит игровое время, отменённый таймер не создаст игрока.
        Time.timeScale = 1f;
        float delay = (float)Field(Find("PlayerRespawner").Single(), "_delayBeforeSpawn");
        yield return new WaitForSeconds(delay + 0.2f);
        Assert.That(Find("Player").Length, Is.EqualTo(1));
        Assert.That(Alive(Player), Is.False);
    }

    [UnityTest]
    public IEnumerator WingsAndSpikes_ConsumeBothEffects_AndStopProcessingTheOldCell()
    {
        var playerBooster = Read(Player, "Boosters");
        var enemyBooster = Read(Enemy, "Boosters");
        Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f));
        Call(Combat, "OnTrailInterrupted", Player, Enemy);
        Assert.That(Alive(Player), Is.True);
        Assert.That(Kills(Enemy), Is.Zero);
        Assert.That(Read(playerBooster, "HasActiveBooster"), Is.EqualTo(false));

        Call(Player, "TryAcceptBooster", New("WingsBoosterEffect", 10f));
        Call(Enemy, "TryAcceptBooster", New("SpikesBoosterEffect", 10f, null));
        var encounter = Hexes.First(hex => Read(hex, "Owner") == null);
        Call(Territory, "TrailHex", Enemy, encounter);
        var conqueror = Part(Player, "Conqueror");
        // Правила трейла живут в TrailRun; вход на клетку вызывается напрямую,
        // чтобы не зависеть от физического перемещения в этом сценарии
        var run = Field(conqueror, "_run");
        Call(run, "Enter", encounter);
        Assert.That(Alive(Player), Is.True);
        Assert.That(Alive(Enemy), Is.True);
        Assert.That(Kills(Enemy), Is.Zero);
        Assert.That(Read(playerBooster, "HasActiveBooster"), Is.EqualTo(false));
        Assert.That(Read(enemyBooster, "HasActiveBooster"), Is.EqualTo(false));
        Assert.That(((IEnumerable)Read(conqueror, "TrailHexes")).Cast<object>(), Is.Empty);
        Assert.That(Read(encounter, "Owner"), Is.SameAs(Enemy));

        Call(Enemy, "TryAcceptBooster", New("SpikesBoosterEffect", 10f, null));
        Call(Combat, "OnTrailInterrupted", Enemy, Player);
        Assert.That(Alive(Player), Is.False);
        Assert.That(Alive(Enemy), Is.True);
        Assert.That(Kills(Enemy), Is.EqualTo(1));
        yield return null;
    }
}
