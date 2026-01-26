using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    [SerializeField] private PlayerStatsView _uiPrefab;

    private void Start()
    {
        var player = SpawnSingleCharacter();
        InitUI(player);
    }
    
    private void InitUI(Player player)
    {
        var statsModel = player.StatsComponent.Stats;
        
        var view = Instantiate(_uiPrefab);
        view.SetCamera(player.Camera);
        
        var presenter = new PlayerStatsPresenter(statsModel, view);
    }
}