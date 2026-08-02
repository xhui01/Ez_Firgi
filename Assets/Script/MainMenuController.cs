using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a GameObject in your MainMenu scene (e.g. Canvas or MainMenuController).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Core reference")]
    [SerializeField] private SceneChanger sceneChanger;

    [Header("Buttons")]
    [SerializeField] private Button mulaButton;         // Start -> world select
    [SerializeField] private Button pencapaianButton;    // Achievements
    [SerializeField] private Button keluarButton;        // Quit

    [Header("Scene names")]
    [Tooltip("The world-select scene shown in your Hierarchy as 'MathWorld'.")]
    [SerializeField] private string mathWorldSceneName = "MathWorld";

    [Tooltip("Leave blank until the Achievement screen is designed.")]
    [SerializeField] private string achievementSceneName = "";

    [Header("Start Options Popup (New Game / Continue)")]
    [SerializeField] private GameObject startWithPopup;  // 'StartWith' Panel
    [SerializeField] private Button newGameButton;       // 'NewGame' button
    [SerializeField] private Button continueButton;      // 'Continue' button

    [Header("Quit & Save Confirmation Popup")]
    [SerializeField] private GameObject saveQuitConfirmationPopup; // Popup panel
    [SerializeField] private Button saveDataAndQuitButton;       // 'Save & Quit' button (YES)
    [SerializeField] private Button dontSaveAndQuitButton;     // 'Don't Save & Quit' button (NO)

    private void Awake()
    {
        // Core buttons
        if (mulaButton != null) mulaButton.onClick.AddListener(OnMulaPressed);
        if (pencapaianButton != null) pencapaianButton.onClick.AddListener(OnPencapaianPressed);
        if (keluarButton != null) keluarButton.onClick.AddListener(OnKeluarPressed);

        // Start options buttons
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGamePressed);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);

        // Quit options buttons
        if (saveDataAndQuitButton != null) saveDataAndQuitButton.onClick.AddListener(OnSaveDataAndQuitPressed);
        if (dontSaveAndQuitButton != null) dontSaveAndQuitButton.onClick.AddListener(OnDontSaveAndQuitPressed);

        // Hide popups by default on start
        if (saveQuitConfirmationPopup != null) saveQuitConfirmationPopup.SetActive(false);
        if (startWithPopup != null) startWithPopup.SetActive(false);
    }

    /// <summary>
    /// MULA (Start) - Checks if player history exists.
    /// Shows New Game / Continue popup if save data is found, otherwise starts immediately.
    /// </summary>
    private void OnMulaPressed()
    {
        if (HasSavedHistory())
        {
            if (startWithPopup != null)
            {
                startWithPopup.SetActive(true);
            }
            else
            {
                sceneChanger.ChangeScene(mathWorldSceneName);
            }
        }
        else
        {
            sceneChanger.ChangeScene(mathWorldSceneName);
        }
    }

    /// <summary>
    /// Checks if any completed levels exist in PlayerPrefs across all 4 worlds.
    /// </summary>
    private bool HasSavedHistory()
    {
        string[] worlds = { "Add", "Minus", "Multiply", "Divide" };

        foreach (string world in worlds)
        {
            for (int level = 1; level <= 10; level++)
            {
                if (PlayerPrefs.GetInt($"{world}_Level{level}_Completed", 0) == 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// NEW GAME - Erases all saved records and starts fresh.
    /// </summary>
    public void OnNewGamePressed()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (startWithPopup != null) startWithPopup.SetActive(false);
        sceneChanger.ChangeScene(mathWorldSceneName);
    }

    /// <summary>
    /// CONTINUE - Keeps existing saved records intact and opens MathWorld.
    /// </summary>
    public void OnContinuePressed()
    {
        if (startWithPopup != null) startWithPopup.SetActive(false);
        sceneChanger.ChangeScene(mathWorldSceneName);
    }

    /// <summary>
    /// PENCAPAIAN (Achievements) - opens the Achievement scene.
    /// </summary>
    private void OnPencapaianPressed()
    {
        if (string.IsNullOrEmpty(achievementSceneName))
        {
            Debug.Log("Pencapaian pressed - Achievement scene not built yet.");
            return;
        }

        sceneChanger.ChangeScene(achievementSceneName);
    }

    /// <summary>
    /// KELUAR (Quit) - Shows the popup asking if player wants to save before quitting.
    /// </summary>
    private void OnKeluarPressed()
    {
        if (saveQuitConfirmationPopup != null)
        {
            saveQuitConfirmationPopup.SetActive(true);
        }
        else
        {
            sceneChanger.QuitApplication();
        }
    }

    /// <summary>
    /// SAVE DATA & QUIT - Flushes all PlayerPrefs data to disk and quits the app.
    /// </summary>
    public void OnSaveDataAndQuitPressed()
    {
        PlayerPrefs.Save(); // Writes all pending changes to disk safely
        sceneChanger.QuitApplication();
    }

    /// <summary>
    /// DON'T SAVE & QUIT - Clears save data and quits without saving.
    /// </summary>
    public void OnDontSaveAndQuitPressed()
    {
        PlayerPrefs.DeleteAll(); // Wipe data so session progress isn't stored
        PlayerPrefs.Save();
        sceneChanger.QuitApplication();
    }
}