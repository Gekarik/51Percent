using System;
using UnityEngine;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private PlayerStatsView _uiPrefab;
    [SerializeField] private HexGrid _grid;

    private void Start()
    {
        if (_grid == null)
            throw new InvalidOperationException($"HexGrid is empty");

        var startHex = _grid.GetRandomHex();
        SpawnSinglePlayer(startHex);
    }

    private void SpawnSinglePlayer(Hex startHex)
    {
        var player = Instantiate(_playerPrefab, startHex.transform.position, Quaternion.identity);
        player.Init(startHex,_grid);

        var statsModel = player.StatsComponent.Stats;

        var view = Instantiate(_uiPrefab);
        view.SetCamera(player.Camera);

        var presenter = new PlayerStatsPresenter(statsModel, view);
    }
}