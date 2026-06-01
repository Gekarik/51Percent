public class HexPresenter
{
    private readonly IHexView _view;
    private IHex _currentHex;
    private ITrailVisualProvider _provider;

    public HexPresenter(IHexView view)
    {
        _view = view;
    }

    public void OnStateChanged(IHex hex)
    {
        _currentHex = hex;
        UpdateProviderSubscription(hex.State == HexState.PartOfTrail ? hex.Owner?.TrailVisual : null);

        switch (hex.State)
        {
            case HexState.Empty:
                _view.SetMesh(null);
                _view.SetOutline(false);
                _view.ResetColor();
                break;

            case HexState.Busy:
                _view.SetMesh(null);
                _view.SetOutline(false);
                _view.SetColorSlowly(hex.Owner.Color);
                break;

            case HexState.PartOfTrail:
                _view.SetMesh(hex.Owner.TrailVisual.ActiveMesh);
                _view.SetOutline(true);
                _view.Pulse();
                _view.SetColorInstantly(hex.Owner.Color);
                break;
        }
    }

    public void Reset()
    {
        UpdateProviderSubscription(null);
        _view.Reset();
    }

    private void UpdateProviderSubscription(ITrailVisualProvider provider)
    {
        if (_provider == provider) return;
        if (_provider != null) _provider.Changed -= OnTrailVisualChanged;
        _provider = provider;
        if (_provider != null) _provider.Changed += OnTrailVisualChanged;
    }

    private void OnTrailVisualChanged()
    {
        _view.SetMesh(_provider.ActiveMesh);
    }
}
