using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Wires the three bottom-bar buttons (Home / Replay / Pause) for a level scene.
/// Pause now loads a SHARED "PauseOverlay" scene additively, so every world
/// (Add/Minus/Multiply/Divide) reuses the exact same pause UI instead of each
/// having its own duplicated panel.
/// Attach this to any GameObject in the level scene and wire references in the Inspector.
/// </summary>
public class LevelUIButtons : MonoBehaviour
{
    [Header("Core reference")]
    [SerializeField] private GameManager gameManager;

    [Header("Bottom bar buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button pauseButton;

    [Header("Scene names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string pauseOverlaySceneName = "PausePage";

    private void Awake()
    {
        if (homeButton != null) homeButton.onClick.AddListener(GoHome);
        if (replayButton != null) replayButton.onClick.AddListener(Replay);
        if (pauseButton != null) pauseButton.onClick.AddListener(OpenPause);
    }

    private void GoHome()
    {
        gameManager.GoToMainMenu(mainMenuSceneName);
    }

    private void Replay()
    {
        gameManager.RestartLevel();
    }

    private void OpenPause()
    {
        gameManager.PauseGame();

        // Don't double-load if it's somehow already open (e.g. double-tap)
        if (!SceneManager.GetSceneByName(pauseOverlaySceneName).isLoaded)
        {
            SceneManager.LoadScene(pauseOverlaySceneName, LoadSceneMode.Additive);
        }
    }
}
