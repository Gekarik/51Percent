using System;

public interface IGrabbableView
{
    event Action AnimationCompleted;
    void PlayCollectAnimation();
    void ResetViewState();
    void DisableView();
}
