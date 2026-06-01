using System;

public interface ICollectibleView
{
    event Action AnimationCompleted;
    void PlayCollectAnimation();
    void ResetViewState();
    void DisableView();
}
