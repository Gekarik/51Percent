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

    // Воскрешение = полноценный новый персонаж; точка спавна исчерпана первым спавном,
    // поэтому место выбирает SpawnHexSelector: свободная область подальше от живых участников
    public Player Respawn()
    {
        return SpawnPlayer();
    }

    // Презентацию нового персонажа подхватывает PlayerHudBinder по событию CharacterSpawned
    private Player SpawnPlayer()
    {
        var player = SpawnNext();
        player.SetName(PlayerName);
        RegisterInLeaderBoard(player);
        return player;
    }
}
