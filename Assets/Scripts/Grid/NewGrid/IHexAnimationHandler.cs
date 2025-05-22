using DG.Tweening;

public interface IHexAnimationHandler
{
    Tween Bounce();
    Tween Stretch();
    void Stop();
    void Reset();
}
