using System;
using System.Reflection;
using NUnit.Framework;

public class MatchStateTests
{
    private object _match;
    private Type _type;
    private int _notifications;

    [SetUp]
    public void SetUp()
    {
        _type = Assembly.Load("51Percent.Runtime").GetType("MatchState", throwOnError: true);
        _match = Activator.CreateInstance(_type);
        _notifications = 0;
        _type.GetEvent("Changed").AddEventHandler(_match, new Action(() => _notifications++));
    }

    [Test]
    public void RepeatedPauseAndResume_NotifyOnlyActualTransitions()
    {
        Assert.That(Read("IsRunning"), Is.True);
        Assert.That(Call("TryResume"), Is.False);
        Assert.That(Call("TryPause"), Is.True);
        Assert.That(Call("TryPause"), Is.False);
        Assert.That(Read("IsPaused"), Is.True);
        Assert.That(Call("TryResume"), Is.True);
        Assert.That(Read("IsRunning"), Is.True);
        Assert.That(_notifications, Is.EqualTo(2));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void FinishedMatch_CannotResumePauseOrFinishAgain(bool paused)
    {
        if (paused)
            Call("TryPause");
        int before = _notifications;
        Assert.That(Call("TryFinish"), Is.True);
        Assert.That(Read("IsFinished"), Is.True);
        Assert.That(Call("TryResume"), Is.False);
        Assert.That(Call("TryPause"), Is.False);
        Assert.That(Call("TryFinish"), Is.False);
        Assert.That(_notifications, Is.EqualTo(before + 1));
    }

    private bool Call(string name) => (bool)_type.GetMethod(name).Invoke(_match, null);
    private bool Read(string name) => (bool)_type.GetProperty(name).GetValue(_match);
}
