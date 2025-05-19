using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private HexGrid _hexgrid;
    [SerializeField] private int _playerCount;
    [SerializeField] private Player _PlayerPrefab;
    [SerializeField] private Camera _camera;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        SpawnPlayers();
    }

    private void SpawnPlayer()
    {
        Hex hex = _hexgrid.GetRandomHex();

        Player player = Instantiate(_PlayerPrefab, hex.transform.position, Quaternion.identity);
        Camera cam = Instantiate(_camera);

        cam.GetComponent<CameraFollower>().Init(player.transform);
        player.Init(hex);
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < _playerCount; i++)
            SpawnPlayer();
    }
}
