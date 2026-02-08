using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PauseWindow _pauseWindowPrefab;
    [SerializeField] private RectTransform _windowsContainer;

    private PauseWindow _pauseWindow;
    private bool _isPaused;

    private void Awake()
    {
        _pauseWindow = Instantiate(_pauseWindowPrefab, _windowsContainer);
        _pauseWindow.Hide();

        _pauseWindow.ContinueClicked += Unpause;
        _pauseWindow.RestartClicked += Restart;
        _pauseWindow.ExitClicked += ExitGame;
    }

    private void OnDestroy()
    {
        if (_pauseWindow == null)
            return;

        _pauseWindow.ContinueClicked -= Unpause;
        _pauseWindow.RestartClicked -= Restart;
        _pauseWindow.ExitClicked -= ExitGame;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // В WebGL нельзя закрыть вкладку через Application.Quit.
        // Можно вызвать JS или просто вернуть в главное меню, когда оно появится.
        Debug.Log("Exit is not supported in WebGL builds");
#else
        Application.Quit();
#endif
    }
}
