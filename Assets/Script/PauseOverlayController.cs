using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a GameObject inside your "PausePage" scene (e.g. the "Pause" panel object).
/// Drag your buttons into the fields below - no need to touch each Button's own OnClick()
/// list in the Inspector, this script wires them all in code via Awake().
///
/// Finds the currently-active level's GameManager via GameManager.Instance (a static
/// singleton set by whichever world scene - AddWorld/MinusWorld/etc - is loaded underneath),
/// so PausePage never needs its own GameManager or a cross-scene Inspector reference.
/// </summary>
public class PauseOverlayController : MonoBehaviour
{
    [Header("Buttons inside this scene")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button closeButton;     // the green X - behaves same as Resume
    [SerializeField] private Button quitButton;       // "back to main menu"

    [Header("Sound - two separate buttons, toggled via SetActive")]
    [SerializeField] private GameObject musicOnButton;   // shown while music IS playing
    [SerializeField] private GameObject musicOffButton;  // shown while music IS muted

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (closeButton != null) closeButton.onClick.AddListener(Resume); // X button = same as Resume
        if (quitButton != null) quitButton.onClick.AddListener(Quit);

        if (musicOnButton != null)
            musicOnButton.GetComponent<Button>().onClick.AddListener(ToggleSound);
        if (musicOffButton != null)
            musicOffButton.GetComponent<Button>().onClick.AddListener(ToggleSound);
    }

    private void Start()
    {
        RefreshSoundButtons();
    }

    private void Resume()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private void Quit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void ToggleSound()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.ToggleMute();
        RefreshSoundButtons();
    }

    private void RefreshSoundButtons()
    {
        if (musicOnButton == null || musicOffButton == null || AudioManager.Instance == null) return;

        bool isMuted = AudioManager.Instance.IsMuted;
        musicOnButton.SetActive(!isMuted);   // show speaker-ON icon when NOT muted
        musicOffButton.SetActive(isMuted);   // show speaker-OFF icon when muted
    }
}