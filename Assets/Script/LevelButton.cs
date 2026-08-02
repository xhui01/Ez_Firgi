//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;

///// <summary>
///// Attach this to your level button PREFAB (not to 10 separate scene objects).
///// One prefab gets instantiated 10 times by LevelSelectManager, each configured
///// with its own level number and completed/not-completed sprite.
///// </summary>
//public class LevelButton : MonoBehaviour
//{
//    [Header("References (already on the prefab)")]
//    [SerializeField] private Image buttonImage;
//    [SerializeField] private TMP_Text numberLabel;   // shows "1", "2", etc.
//    [SerializeField] private Button button;

//    [Header("Sprites")]
//    [SerializeField] private Sprite defaultSprite;    // e.g. silver - not completed yet
//    [SerializeField] private Sprite completedSprite;   // e.g. gold - finished this level

//    private int _levelNumber;
//    private string _targetSceneName;

//    /// <summary>
//    /// Called once by LevelSelectManager right after instantiating this prefab.
//    /// </summary>
//    public void Setup(int levelNumber, bool isCompleted, string targetSceneName)
//    {
//        _levelNumber = levelNumber;
//        _targetSceneName = targetSceneName;

//        if (numberLabel != null) numberLabel.text = levelNumber.ToString();
//        if (buttonImage != null) buttonImage.sprite = isCompleted ? completedSprite : defaultSprite;

//        button.onClick.RemoveAllListeners();
//        button.onClick.AddListener(OnClicked);
//    }

//    private void OnClicked()
//    {
//        // Tell the level scene which level number to load - GameManager reads this on Start()
//        PlayerPrefs.SetInt("SelectedLevel", _levelNumber);
//        PlayerPrefs.Save();

//        SceneManager.LoadScene(_targetSceneName);
//    }
//}


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to your level button PREFAB (e.g. the "1" object) - NOT to the
/// LevelButtonContainer. One prefab gets instantiated 10 times by
/// LevelSelectManager, each configured with its OWN pair of sprites
/// (e.g. clone #3 gets S-3/G-3, clone #7 gets S-7/G-7) since every level
/// needs a different number baked into its image.
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Header("References (already on the prefab)")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Button button;

    private int _levelNumber;
    private string _targetSceneName;

    /// <summary>
    /// Called once by LevelSelectManager right after instantiating this prefab.
    /// defaultSprite/completedSprite are passed in per-level (e.g. S-3/G-3 for level 3)
    /// rather than being fixed values baked into the prefab.
    /// </summary>
    public void Setup(int levelNumber, bool isCompleted, string targetSceneName,
                       Sprite defaultSprite, Sprite completedSprite)
    {
        _levelNumber = levelNumber;
        _targetSceneName = targetSceneName;

        if (buttonImage != null)
        {
            buttonImage.sprite = isCompleted ? completedSprite : defaultSprite;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogError($"LevelButton on '{gameObject.name}' has no Button reference assigned in the Inspector.");
        }
    }

    private void OnClicked()
    {
        PlayerPrefs.SetInt("SelectedLevel", _levelNumber);
        PlayerPrefs.Save();

        SceneManager.LoadScene(_targetSceneName);
    }
}