public class PlayerStatsPresenter
{
    private readonly PlayerStats _model;
    private readonly PlayerStatsView _view;

    public PlayerStatsPresenter(PlayerStats model, PlayerStatsView view)
    {
        _model = model;
        _view = view;

        _model.CoinsChanged += _view.UpdateCoins;
        _model.KillsChanged += _view.UpdateKills;

        _view.UpdateCoins(_model.Coins);
        _view.UpdateKills(_model.Kills);
    }

    public void Dispose()
    {
        _model.CoinsChanged -= _view.UpdateCoins;
        _model.KillsChanged -= _view.UpdateKills;
    }
}