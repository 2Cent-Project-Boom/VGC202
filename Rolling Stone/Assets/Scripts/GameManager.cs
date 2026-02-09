using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool gameHasEnded = false;
    [SerializeField] private bool isPaused = false;
    public bool IsPaused => isPaused;

    [Header("Timing")]
    [SerializeField] private float restartDelay = 1f;

    [Header("UI References")]
    public GameObject completeLevelUI;
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;
    public GameObject inGameHUD;

    [Header("Final Distance")]
    public Transform player;
    public TextMeshProUGUI finalDistanceText;

    private string lastGameOverReason = "Unknown";

    private void Awake()
    {
        Time.timeScale = 1f;
        gameHasEnded = false;
        isPaused = false;

        if (gameOverUI) gameOverUI.SetActive(false);
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        if (completeLevelUI) completeLevelUI.SetActive(false);
        if (inGameHUD) inGameHUD.SetActive(true);
    }

    // ----------------------------
    // LEVEL COMPLETE
    // ----------------------------
    public void CompleteLevel()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;

        if (inGameHUD) inGameHUD.SetActive(false);
        if (completeLevelUI) completeLevelUI.SetActive(true);
    }

    // ----------------------------
    // GAME OVER
    // ----------------------------
    public void EndGame()
    {
        EndGame("Unknown");
    }

    public void EndGame(string reason)
    {
        if (gameHasEnded) return;
        gameHasEnded = true;

        lastGameOverReason = string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason;

        // Use Error so Unity shows a stack trace even if Log stack traces are muted.
        Debug.LogError($"GAME OVER: {lastGameOverReason}", this);

        if (restartDelay <= 0f)
        {
            ShowGameOverScreen();
            return;
        }

        Invoke(nameof(ShowGameOverScreen), restartDelay);
    }

    private void ShowGameOverScreen()
    {
        if (player != null && finalDistanceText != null)
        {
            float finalDistance = player.position.z;
            finalDistanceText.text = $"Final Distance {finalDistance:0} m";
        }

        if (inGameHUD) inGameHUD.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    // ----------------------------
    // PAUSE / RESUME
    // ----------------------------
    public void Pause()
    {
        if (gameHasEnded || isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuUI) pauseMenuUI.SetActive(true);
        if (inGameHUD) inGameHUD.SetActive(false);
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        if (inGameHUD) inGameHUD.SetActive(true);
    }

    // ----------------------------
    // MENU NAVIGATION
    // ----------------------------
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
