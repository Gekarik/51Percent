public class PlayerBoosterHudPresenter
{
    private readonly IBoosterObservable _observable;
    private readonly PlayerBoosterHUD _view;
    private readonly BoosterIconRegistry _registry;

    public PlayerBoosterHudPresenter(IBoosterObservable observable, PlayerBoosterHUD view, BoosterIconRegistry registry)
    {
        _observable = observable;
        _view = view;
        _registry = registry;

        _observable.BoosterChanged += HandleBoosterChanged;
        HandleBoosterChanged();
    }

    public void Tick()
    {
        if (!_observable.HasActiveBooster)
            return;

        _view.SetTimer($"{_observable.RemainingTime:F1}");
    }

    public void Dispose()
    {
        _observable.BoosterChanged -= HandleBoosterChanged;
    }

    private void HandleBoosterChanged()
    {
        if (!_observable.HasActiveBooster)
        {
            _view.Hide();
            return;
        }

        var icon = _registry.Get(_observable.ActiveEffect.BoosterId);
        _view.Show(icon);
    }
}
