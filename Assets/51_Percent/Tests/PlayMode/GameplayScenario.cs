using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public abstract class GameplayScenario
{
    private Assembly _gameAssembly;
    private readonly List<Action> _unsubscribe = new List<Action>();
    private readonly List<string> _tweenWarnings = new List<string>();
    private float _previousTimeScale;

    protected Component Player { get; private set; }
    protected Component Enemy { get; private set; }
    protected Component Territory { get; private set; }
    protected Component Grid { get; private set; }
    protected object Match { get; private set; }
    protected object Combat { get; private set; }
    protected object WinConditions { get; private set; }
    protected Component[] Hexes { get; private set; }

    [UnitySetUp]
    public IEnumerator LoadScene()
    {
        _tweenWarnings.Clear();
        Application.logMessageReceived += OnLogMessage;
        // Адаптер к Assembly-CSharp: тестовая assembly не меняет устройство игрового кода.
        _gameAssembly = Assembly.Load("51Percent.Runtime");
        _previousTimeScale = Time.timeScale;
        yield return SceneManager.LoadSceneAsync("SampleScene");
        yield return null;
        Time.timeScale = 0f;

        Player = Find("Player").Single();
        Enemy = Find("Enemy").First();
        Territory = Find("TerritoryManager").Single();
        Grid = Find("HexGrid").Single();
        Hexes = ((IEnumerable)Read(Grid, "AllHexes")).Cast<Component>().ToArray();
        var bootstrap = Find("Bootstrap").Single();
        Match = Field(bootstrap, "_matchState");
        Combat = Field(bootstrap, "_killManager");
        WinConditions = Field(bootstrap, "_winConditionTracker");

        // Окно приземления — модель персонажа, а не компонент сцены
        foreach (var character in new[] { Player, Enemy })
            Call(Field(character, "_landing"), "Cancel");
        FreezeWorld();
    }

    [TearDown]
    public void RestoreTimeAndSubscriptions()
    {
        Application.logMessageReceived -= OnLogMessage;
        foreach (var unsubscribe in _unsubscribe)
            unsubscribe();
        _unsubscribe.Clear();
        Time.timeScale = _previousTimeScale;
        Assert.That(_tweenWarnings, Is.Empty, "Анимация обратилась к уничтоженному объекту или сообщила о другом сбое DOTween.");
    }

    private void OnLogMessage(string message, string stackTrace, LogType type)
    {
        // Safe Mode DOTween переводит исключения в предупреждения: обычный UnityTest их не отклоняет.
        if (type != LogType.Log && message.Contains("DOTWEEN"))
            _tweenWarnings.Add(message);
    }

    protected Component[] Find(string name)
    {
        return UnityEngine.Object.FindObjectsOfType(GameType(name)).Cast<Component>().ToArray();
    }

    protected Component Part(Component owner, string typeName) => owner.GetComponent(GameType(typeName));
    protected Type GameType(string name) => _gameAssembly.GetType(name, throwOnError: true);
    protected object New(string typeName, params object[] args) => Activator.CreateInstance(GameType(typeName), args);

    protected object Read(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        Assert.That(property, Is.Not.Null, name);
        return property.GetValue(target);
    }

    protected object Field(object target, string name)
    {
        return FindField(target, name).GetValue(target);
    }

    protected void SetField(object target, string name, object value)
    {
        FindField(target, name).SetValue(target, value);
    }

    private FieldInfo FindField(object target, string name)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null)
                return field;
        }
        throw new AssertionException("Поле не найдено: " + name);
    }

    protected object Call(object target, string name, params object[] args)
    {
        var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var method = methods.Single(candidate => candidate.Name == name
            && candidate.GetParameters().Length == args.Length
            && candidate.GetParameters().Select((parameter, index) => args[index] == null
                || parameter.ParameterType.IsInstanceOfType(args[index])).All(matches => matches));
        return method.Invoke(target, args);
    }

    protected void Observe(object target, string eventName, Action observer)
    {
        var eventInfo = target.GetType().GetEvent(eventName);
        eventInfo.AddEventHandler(target, observer);
        _unsubscribe.Add(() => eventInfo.RemoveEventHandler(target, observer));
    }

    protected void ObserveResult(object target, string eventName, Action<object> observer)
    {
        var eventInfo = target.GetType().GetEvent(eventName);
        var parameterType = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters().Single().ParameterType;
        var parameter = Expression.Parameter(parameterType);
        var body = Expression.Invoke(Expression.Constant(observer), Expression.Convert(parameter, typeof(object)));
        var callback = Expression.Lambda(eventInfo.EventHandlerType, body, parameter).Compile();
        eventInfo.AddEventHandler(target, callback);
        _unsubscribe.Add(() => eventInfo.RemoveEventHandler(target, callback));
    }

    protected Array HexArray(IEnumerable<Component> hexes)
    {
        var values = hexes.ToArray();
        var result = Array.CreateInstance(GameType("IHex"), values.Length);
        for (int i = 0; i < values.Length; i++)
            result.SetValue(values[i], i);
        return result;
    }

    protected Component[] Neighbors(Component hex)
    {
        return ((IEnumerable)Call(Grid, "GetNeighbors", hex)).Cast<Component>().ToArray();
    }

    protected int OwnedCount(Component character)
    {
        return ((IEnumerable)Call(Territory, "GetFixedByOwner", character)).Cast<object>().Count();
    }

    protected int Kills(Component character) => (int)Read(Read(character, "LifeStats"), "Kills");
    protected bool Alive(Component character) => Read(character, "State").ToString() == "Alive";
    protected IDisposable Changes() => (IDisposable)Call(Territory, "BeginChanges");

    protected void FreezeWorld()
    {
        foreach (string typeName in new[] { "Mover", "Conqueror", "Grabber" })
            foreach (var component in Find(typeName))
                ((Behaviour)component).enabled = false;
    }
}
