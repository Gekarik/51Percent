using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Required] [SerializeField] private PauseWindow _pauseWindowPrefab;
    [Required] [SerializeField] private EndgameWindow _endgameWindowPrefab;
    [Required] [SerializeField] private RectTransform _windowsContainer;

    private PauseWindow _pauseWindow;
    private EndgameWindow _endgameWindow;
    private WinConditionTracker _winConditionTracker;
    private TerritoryManager _territoryManager;
    private MatchState _matchState;

    public void Init(WinConditionTracker winConditionTracker, TerritoryManager territoryManager, MatchState matchState)
    {
        _winConditionTracker = winConditionTracker;
        _territoryManager = territoryManager;
        _matchState = matchState;
        _winConditionTracker.GameFinished += OnGameFinished;
        _matchState.Changed += OnMatchStateChanged;
    }

    private void Awake()
    {
        _pauseWindow = Instantiate(_pauseWindowPrefab, _windowsContainer);
        _pauseWindow.Hide();

        _endgameWindow = Instantiate(_endgameWindowPrefab, _windowsContainer);
        _endgameWindow.Hide();

        _pauseWindow.ContinueClicked += Unpause;
        _pauseWindow.RestartClicked += Restart;
        _pauseWindow.ExitClicked += ExitGame;

        _endgameWindow.RestartClicked += Restart;
        _endgameWindow.ExitClicked += ExitGame;
        OnMatchStateChanged();
    }

    private void OnDestroy()
    {
        if (_pauseWindow != null)
        {
            _pauseWindow.ContinueClicked -= Unpause;
            _pauseWindow.RestartClicked -= Restart;
            _pauseWindow.ExitClicked -= ExitGame;
        }

        if (_endgameWindow != null)
        {
            _endgameWindow.RestartClicked -= Restart;
            _endgameWindow.ExitClicked -= ExitGame;
        }

        if (_winConditionTracker != null)
            _winConditionTracker.GameFinished -= OnGameFinished;

        if (_matchState != null)
            _matchState.Changed -= OnMatchStateChanged;
    }

    private void Update()
    {
        if (_matchState == null || _matchState.IsFinished)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnGameFinished(ICharacter winner)
    {
        bool isVictory = winner.IsHuman;
        float territory = _territoryManager.GetOwnershipPercent(winner);

        var stats = winner.LifeStats;

        _endgameWindow.Show(winner.Name, winner.Color, isVictory, territory, stats.Kills, stats.Coins);
    }

    private void TogglePause()
    {
        if (_matchState.IsPaused)
            Unpause();
        else
            Pause();
    }

    private void Pause()
    {
        _matchState.TryPause();
    }

    private void Unpause()
    {
        _matchState.TryResume();
    }

    private void OnMatchStateChanged()
    {
        if (_matchState == null)
            return;

        Time.timeScale = _matchState.IsRunning ? 1f : 0f;
        if (_pauseWindow == null)
            return;

        if (_matchState.IsPaused)
            _pauseWindow.Show();
        else
            _pauseWindow.Hide();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
