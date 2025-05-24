using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid _hexGrid;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Camera _cameraPrefab;

    [SerializeField] private float _spawnDelay = 2f;
    [SerializeField] private int _playerCount = 2;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_spawnDelay);
        StartCoroutine(nameof(SpawnPlayers));
    }

    private IEnumerator SpawnPlayers()
    {
        for (int i = 0; i < _playerCount; i++)
        {
            yield return new WaitForSeconds(_spawnDelay);
            SpawnSinglePlayer();
        }
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
