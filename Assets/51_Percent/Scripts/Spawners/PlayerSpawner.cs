using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    [SerializeField] private PlayerStatsView _uiPrefab;

    private PlayerStatsPresenter _presenter;

    private void Start()
    {
        EnsureInitialized();
        var player = SpawnSingleCharacter();
        InitUI(player);
    }

    private void InitUI(Player player)
    {
        var statsModel = player.StatsComponent.Stats;

        var view = Instantiate(_uiPrefab);
        view.SetCamera(player.Camera);

        _presenter = new PlayerStatsPresenter(statsModel, view);
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
    }
}
