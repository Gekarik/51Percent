using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CharacterLifeStatsTests
{
    private Assembly _gameAssembly;
    private float _previousTimeScale;

    [SetUp]
    public void SetUp()
    {
        // Игровой код пока в Assembly-CSharp: отдельная тестовая assembly не может
        // ссылаться на неё напрямую. Reflection ограничен адаптером к существующей сцене.
        _gameAssembly = Assembly.Load("Assembly-CSharp");
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = _previousTimeScale;
    }

    [UnityTest]
    public IEnumerator DeathAndRespawn_ResetLifeStats_RebindHud_AndPreserveWinnerStats()
    {
        yield return SceneManager.LoadSceneAsync("SampleScene");
        yield return null;

        var player = Find("Player").Single();
        var enemy = Find("Enemy").First();
        var oldStats = ReadProperty(player, "LifeStats");
        var enemyStats = ReadProperty(enemy, "LifeStats");
        var view = Find("PlayerStatsView").Single();

        Assert.That(oldStats, Is.Not.SameAs(enemyStats));
        AssertStats(oldStats, 0, 0);
        AssertStats(enemyStats, 0, 0);
        AssertHud(view, 0, 0);

        Call(player, "AcceptCoin");
        Call(player, "AcceptCoin");
        Call(player, "Kill");
        AssertStats(oldStats, 2, 1);
        AssertStats(enemyStats, 0, 0);
        AssertHud(view, 2, 1);

        var bootstrap = Find("Bootstrap").Single();
        var respawner = Find("PlayerRespawner").Single();
        var killManager = ReadField(bootstrap, "_killManager");
        int coinsBeforeDeath = Find("Coin").Length;

        // Смерть проходит через действующую цепочку, включая рассыпание и отложенный респавн.
        Call(killManager, "OnTrailInterrupted", player, enemy);
        Assert.That(ReadProperty(player, "State").ToString(), Is.EqualTo("Died"));
        Assert.That(ReadProperty(respawner, "HasPendingRespawn"), Is.EqualTo(true));
        Assert.That(Find("Coin").Length, Is.EqualTo(coinsBeforeDeath + 2));
        AssertStats(oldStats, 2, 1);
        AssertStats(enemyStats, 0, 1);

        Component respawned = null;
        float deadline = Time.realtimeSinceStartup + 10f;
        Time.timeScale = 1f;
        while (respawned == null && Time.realtimeSinceStartup < deadline)
        {
            // Изолируем проверяемый цикл от случайных перемещений ботов и подбора предметов.
            foreach (string typeName in new[] { "Mover", "Conqueror", "Grabber" })
                foreach (var component in Find(typeName))
                    ((Behaviour)component).enabled = false;

            yield return null;
            respawned = Find("Player").FirstOrDefault(candidate => candidate != player);
        }
        Time.timeScale = 0f;

        Assert.That(respawned, Is.Not.Null, "Игрок должен появиться через PlayerRespawner.");
        Assert.That(ReadProperty(respawner, "HasPendingRespawn"), Is.EqualTo(false));
        var newStats = ReadProperty(respawned, "LifeStats");
        Assert.That(newStats, Is.Not.SameAs(oldStats));
        AssertStats(newStats, 0, 0);
        Assert.That(Find("PlayerStatsView").Single(), Is.SameAs(view));
        AssertHud(view, 0, 0);

        // Старая модель уже не должна обновлять сохранённую вьюшку HUD.
        Call(oldStats, "AddCoin");
        Call(oldStats, "AddKill");
        AssertHud(view, 0, 0);
        Call(respawned, "AcceptCoin");
        Call(respawned, "Kill");
        AssertStats(newStats, 1, 1);
        AssertHud(view, 1, 1);

        // Финальное окно должно взять данные победившего бота, а не модель HUD игрока.
        Call(enemy, "AcceptCoin");
        Call(enemy, "AcceptCoin");
        Call(enemy, "AcceptCoin");
        Call(ReadField(bootstrap, "_winConditionTracker"), "ForceFinish", enemy);
        var endgame = Find("EndgameWindow").Single();
        Assert.That(ReadText(endgame, "_coinsText"), Is.EqualTo("3"));
        Assert.That(ReadText(endgame, "_killsText"), Is.EqualTo("1"));
        Assert.That(ReadText(endgame, "_winnerNameText"), Is.EqualTo(ReadProperty(enemy, "Name")));
        LogAssert.NoUnexpectedReceived();
    }

    private Component[] Find(string typeName)
    {
        var type = _gameAssembly.GetType(typeName, throwOnError: true);
        return UnityEngine.Object.FindObjectsOfType(type).Cast<Component>().ToArray();
    }

    private object ReadProperty(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        Assert.That(property, Is.Not.Null, name);
        return property.GetValue(target);
    }

    private object ReadField(object target, string name)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, name);
        return field.GetValue(target);
    }

    private void Call(object target, string methodName, params object[] arguments)
    {
        var method = target.GetType().GetMethod(methodName);
        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, arguments);
    }

    private string ReadText(object view, string fieldName)
    {
        return (string)ReadProperty(ReadField(view, fieldName), "text");
    }

    private void AssertStats(object stats, int coins, int kills)
    {
        Assert.That(ReadProperty(stats, "Coins"), Is.EqualTo(coins));
        Assert.That(ReadProperty(stats, "Kills"), Is.EqualTo(kills));
    }

    private void AssertHud(object view, int coins, int kills)
    {
        Assert.That(ReadText(view, "_coinsText"), Is.EqualTo($"Coins: {coins}"));
        Assert.That(ReadText(view, "_killsText"), Is.EqualTo($"Kills: {kills}"));
    }
}
