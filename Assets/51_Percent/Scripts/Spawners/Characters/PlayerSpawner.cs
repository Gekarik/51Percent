using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    private const string PlayerName = "Player";

    [SerializeField] private Transform _spawnPoint;

    private void Start()
    {
        EnsureInitialized();
        var hex = (_spawnPoint != null ? Grid.GetHexAt(_spawnPoint.position) : null) ?? Grid.GetRandomHex();
        SetSpawnHexes(new[] { hex });
        SpawnPlayer();
    }

    public Player Respawn()
    {
        return SpawnPlayer();
    }

    private Player SpawnPlayer()
    {
        var player = SpawnNext();
        player.SetName(PlayerName);
        RegisterInLeaderBoard(player);
        return player;
    }
}
