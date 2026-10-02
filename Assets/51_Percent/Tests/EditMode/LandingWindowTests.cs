using NUnit.Framework;

// Срок окна приземления — правило персонажа, поэтому проверяется без сцены и без кадров.
public class LandingWindowTests
{
    private const float Duration = 0.75f;

    private LandingWindow _window;
    private int _started;
    private int _finished;

    [SetUp]
    public void SetUp()
    {
        _started = 0;
        _finished = 0;
        _window = new LandingWindow(Duration);
        _window.Started += () => _started++;
        _window.Finished += () => _finished++;
    }

    [Test]
    public void NewWindow_IsClosedUntilBegin()
    {
        Assert.That(_window.IsLanding, Is.False);

        _window.Begin();

        Assert.That(_window.IsLanding, Is.True);
        Assert.That(_started, Is.EqualTo(1));
    }

    [Test]
    public void Window_ClosesExactlyAfterItsDuration()
    {
        _window.Begin();

        _window.Tick(Duration - 0.01f);
        Assert.That(_window.IsLanding, Is.True, "Окно закрылось раньше срока.");

        _window.Tick(0.01f);
        Assert.That(_window.IsLanding, Is.False);
        Assert.That(_window.Progress, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(_finished, Is.EqualTo(1));
    }

    [Test]
    public void PausedTime_DoesNotAdvanceWindow()
    {
        _window.Begin();
        _window.Tick(0.2f);
        float progress = _window.Progress;

        for (int i = 0; i < 50; i++)
            _window.Tick(0f);

        Assert.That(_window.Progress, Is.EqualTo(progress).Within(0.0001f));
        Assert.That(_window.IsLanding, Is.True);
    }

    [Test]
    public void Cancel_ClosesWindowOnce_LikeNaturalExpiry()
    {
        _window.Begin();

        _window.Cancel();
        _window.Cancel();

        Assert.That(_window.IsLanding, Is.False);
        Assert.That(_finished, Is.EqualTo(1), "Повторная отмена снова уведомила подписчиков.");
    }

    [Test]
    public void TickAfterClose_DoesNotNotifyAgain()
    {
        _window.Begin();
        _window.Tick(Duration);

        _window.Tick(Duration);

        Assert.That(_finished, Is.EqualTo(1));
    }
}
