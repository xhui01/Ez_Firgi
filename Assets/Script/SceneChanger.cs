using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generic scene-changing helper. Attach to any GameObject (e.g. a "SceneChanger"
/// object in MainMenu, or directly on each world-select button) and wire button
/// OnClick events to these public methods.
///
/// Works for: MainMenu -> AddWorld/MinusWorld/MultiplyWorld/DivideWorld,
/// world-select -> level-select, back buttons, etc.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by name, replacing all currently loaded scenes (Single mode).
    /// This is what you want for normal navigation - e.g. MainMenu button ->
    /// "AddWorld". Also automatically unloads any additive scenes (like PausePage)
    /// if one happened to still be loaded.
    /// Wire this directly to a Button's OnClick() - drag this component in,
    /// select SceneChanger -> ChangeScene, then type the scene name in the field.
    /// </summary>
    public void ChangeScene(string sceneName)
    {
        Debug.Log($"ChangeScene() called with: '{sceneName}'");   // <-- add this line

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneChanger.ChangeScene called with an empty scene name.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Same as ChangeScene, but by build index instead of name - useful if you
    /// prefer wiring buttons to fixed positions in File > Build Settings.
    /// </summary>
    public void ChangeSceneByIndex(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// Reloads whichever scene is currently active - handy for a generic "Retry"
    /// button anywhere that isn't already covered by GameManager.RestartLevel().
    /// </summary>
    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// Loads a scene additively (stacks on top, doesn't unload the current one) -
    /// e.g. for popups/overlays similar to PausePage.
    /// </summary>
    public void ChangeSceneAdditive(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// Unloads a specific additively-loaded scene by name - pairs with ChangeSceneAdditive.
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    /// <summary>
    /// Quits the application. Only works in a real build - does nothing useful
    /// in the Editor (Unity just logs a message instead of actually closing).
    /// </summary>
    public void QuitApplication()
    {
        Application.Quit();
    }
}
