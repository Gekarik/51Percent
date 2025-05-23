using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid _hexGrid;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Camera _cameraPrefab;

    [SerializeField] private float _spawnDelay = 2f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_spawnDelay);
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        SpawnSinglePlayer();
    }

    private void SpawnSinglePlayer()
    {
        var hex = _hexGrid.GetRandomHex();

        var player = Instantiate(_playerPrefab, hex.transform.position, Quaternion.identity);
        player.Init(hex, _hexGrid);

        var cam = Instantiate(_cameraPrefab);
        var follower = cam.GetComponent<CameraFollower>();

        if (follower != null)
            follower.Init(player.transform);
    }
}
