using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PauseWindow _pauseWindowPrefab;
    [SerializeField] private EndgameWindow _endgameWindowPrefab;
    [SerializeField] private RectTransform _windowsContainer;

    private PauseWindow _pauseWindow;
    private EndgameWindow _endgameWindow;
    private WinConditionTracker _winConditionTracker;
    private TerritoryManager _territoryManager;
    private bool _isPaused;
    private bool _isGameOver;

    public void Init(WinConditionTracker winConditionTracker, TerritoryManager territoryManager)
    {
        _winConditionTracker = winConditionTracker;
        _territoryManager = territoryManager;
        _winConditionTracker.GameFinished += OnGameFinished;
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
    }

    private void Update()
    {
        if (_isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnGameFinished(ICharacter winner)
    {
        _isGameOver = true;
        Time.timeScale = 0f;

        bool isVictory = winner is Player;
        float territory = _territoryManager.GetOwnershipPercent(winner);

        var stats = winner.StatsComponent.Stats;

        _endgameWindow.Show(winner.Name, winner.Color, isVictory, territory, stats.Kills, stats.Coins);
    }

    private void TogglePause()
    {
        if (_isPaused)
            Unpause();
        else
            Pause();
    }

    private void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _pauseWindow.Show();
    }

    private void Unpause()
    {
        _isPaused = false;
        Time.timeScale = 1f;
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
