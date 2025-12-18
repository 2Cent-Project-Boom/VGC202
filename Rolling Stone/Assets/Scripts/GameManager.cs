using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool gameHasEnded = false;
    [SerializeField] private bool isPaused = false;

    public bool IsPaused => isPaused;

    [Header("Timing")]
    [SerializeField] private float restartDelay = 1f;

    [Header("UI References")]
    [Tooltip("Panel shown when the level is completed (you already had this).")]
    public GameObject completeLevelUI;

    [Tooltip("Panel shown when the player dies / falls / hits obstacle.")]
    public GameObject gameOverUI;

    [Tooltip("Panel shown when the game is paused.")]
    public GameObject pauseMenuUI;

    [Tooltip("HUD shown during gameplay (score, pause button, etc.).")]
    public GameObject inGameHUD;

    [Header("Final Distance")]
    [Tooltip("Player transform used to read final distance (Z position).")]
    public Transform player;

    [Tooltip("UI Text on the Game Over screen that shows the final distance.")]
    public TextMeshProUGUI finalDistanceText;

    private void Awake()
    {
        Time.timeScale = 1f;
        gameHasEnded = false;
        isPaused = false;

        if (gameOverUI) gameOverUI.SetActive(false);
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
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
        if (gameHasEnded) return;
        gameHasEnded = true;

        Debug.Log("GAME OVER");
        Invoke(nameof(ShowGameOverScreen), restartDelay);
    }

    private void ShowGameOverScreen()
    {
        // Calculate and display final distance
        if (player != null && finalDistanceText != null)
        {
            float finalDistance = player.position.z;
            finalDistanceText.text = $"Final Distance {finalDistance:0} m";
        }


        if (inGameHUD) inGameHUD.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(true);

        Time.timeScale = 0f; // freeze everything
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
