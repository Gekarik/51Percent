using UnityEngine;

public class CrownController : MonoBehaviour
{
    [SerializeField] private Crown _crownPrefab;

    private LeaderBoardModel _leaderBoardModel;
    private Crown _crown;
    private ICharacter _currentLeader;

    public void Init(LeaderBoardModel leaderBoardModel)
    {
        _leaderBoardModel = leaderBoardModel;
        _crown = Instantiate(_crownPrefab);
        _crown.gameObject.SetActive(false);
        _leaderBoardModel.Changed += OnLeaderboardChanged;
    }

    private void OnDestroy()
    {
        if (_leaderBoardModel != null)
            _leaderBoardModel.Changed -= OnLeaderboardChanged;
    }

    private void OnLeaderboardChanged()
    {
        var entries = _leaderBoardModel.Entries;

        if (entries.Count == 0)
        {
            _currentLeader = null;
            _crown.gameObject.SetActive(false);
            return;
        }

        var newLeader = entries[0].Character;

        if (newLeader == _currentLeader) return;

        _currentLeader = newLeader;

        var socket = _currentLeader.GetSocket(SocketType.Head);
        _crown.transform.SetParent(socket, worldPositionStays: false);
        _crown.transform.localPosition = Vector3.zero;
        _crown.transform.localRotation = Quaternion.identity;
        _crown.gameObject.SetActive(true);
    }
}
