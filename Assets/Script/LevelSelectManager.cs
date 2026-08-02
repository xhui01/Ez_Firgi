using UnityEngine;

/// <summary>
/// Attach this to the parent Panel that holds all 10 level buttons.
/// Make sure the container has a GRID LAYOUT GROUP component attached.
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    public enum WorldType { Add, Minus, Multiply, Divide }

    [System.Serializable]
    public struct WorldConfig
    {
        public WorldType worldType;
        public string worldKey;        // Unique identifier for PlayerPrefs saving
        public string targetSceneName;  // The gameplay scene to load when a level is clicked
    }

    [Header("Prefab + Container")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject levelButtonPrefab;

    [Header("Level Config")]
    [SerializeField] private int totalLevels = 10;

    [Header("World Configurations")]
    [SerializeField]
    private WorldConfig[] worlds = new WorldConfig[]
    {
        new WorldConfig { worldType = WorldType.Add,      worldKey = "Add",      targetSceneName = "AddQuestions" },
        new WorldConfig { worldType = WorldType.Minus,    worldKey = "Minus",    targetSceneName = "MinusQuestions" },
        new WorldConfig { worldType = WorldType.Multiply, worldKey = "Multiply", targetSceneName = "MultiplyQuestions" },
        new WorldConfig { worldType = WorldType.Divide,   worldKey = "Divide",   targetSceneName = "DivideQuestions" }
    };

    [Header("Currently Active World")]
    [SerializeField] private WorldType currentWorld = WorldType.Add;

    [Header("Per-Level Sprites (index 0 = Level 1, index 9 = Level 10)")]
    [SerializeField] private Sprite[] defaultSprites = new Sprite[10];
    [SerializeField] private Sprite[] completedSprites = new Sprite[10];

    private void Start()
    {
        // 1. Retrieve the world selected from the WorldSelectMenu screen
        currentWorld = WorldSelectionState.CurrentSelectedWorld;

        // 2. Generate and display the buttons for this active world
        LoadWorld(currentWorld);
    }

    /// <summary>
    /// Spawns and sets up level buttons specifically for the chosen world.
    /// </summary>
    public void LoadWorld(WorldType selectedWorld)
    {
        currentWorld = selectedWorld;

        // Find matching configuration data for the selected world
        WorldConfig activeConfig = GetConfigForWorld(selectedWorld);

        // Clear existing spawned buttons to prevent duplicates
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate level buttons for the active world
        for (int i = 1; i <= totalLevels; i++)
        {
            // Instantiate prefab safely for UI Canvas elements
            GameObject buttonObj = Instantiate(levelButtonPrefab);
            buttonObj.transform.SetParent(buttonContainer, false);
            buttonObj.transform.localScale = Vector3.one;

            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();

            if (levelButton == null)
            {
                Debug.LogError($"Instantiated button for level {i} is missing the LevelButton component!");
                continue;
            }

            // Check completion progress using the active world's unique key
            bool isCompleted = IsLevelCompleted(activeConfig.worldKey, i);

            // Fetch sprite visuals for level i
            int index = i - 1;
            Sprite defaultSprite = index < defaultSprites.Length ? defaultSprites[index] : null;
            Sprite completedSprite = index < completedSprites.Length ? completedSprites[index] : null;

            // Configure button actions for THIS specific world
            levelButton.Setup(i, isCompleted, activeConfig.targetSceneName, defaultSprite, completedSprite);
        }
    }

    private WorldConfig GetConfigForWorld(WorldType type)
    {
        foreach (var config in worlds)
        {
            if (config.worldType == type) return config;
        }
        return worlds[0]; // Fallback to first config if missing
    }

    private bool IsLevelCompleted(string worldKey, int levelNumber)
    {
        return PlayerPrefs.GetInt(CompletionKey(worldKey, levelNumber), 0) == 1;
    }

    private string CompletionKey(string worldKey, int levelNumber)
    {
        return $"{worldKey}_Level{levelNumber}_Completed";
    }

    /// <summary>
    /// Call this from your gameplay/question manager when a level is finished:
    /// e.g. LevelSelectManager.MarkLevelCompleted("Minus", 1);
    /// </summary>
    public static void MarkLevelCompleted(string worldKey, int levelNumber)
    {
        PlayerPrefs.SetInt($"{worldKey}_Level{levelNumber}_Completed", 1);
        PlayerPrefs.Save();
    }
}


//using UnityEngine;

///// <summary>
///// Attach this to the parent Panel that will hold all 10 level buttons -
///// that same Panel should have a GRID LAYOUT GROUP component added to it
///// (Component > Layout > Grid Layout Group), which handles the neat
///// auto-arranging so you never position buttons by hand again.
///// </summary>
//public class LevelSelectManager : MonoBehaviour
//{
//    public enum WorldType { Add, Minus, Multiply, Divide }

//    [System.Serializable]
//    public struct WorldConfig
//    {
//        public WorldType worldType;
//        public string worldKey;       // e.g., "Add"
//        public string targetSceneName; // e.g., "AddQuestions"
//    }

//    [Header("Prefab + Container")]
//    [SerializeField] private Transform buttonContainer;
//    [SerializeField] private GameObject levelButtonPrefab;

//    [Header("Level Config")]
//    [SerializeField] private int totalLevels = 10;

//    [Header("World Configurations")]
//    [SerializeField]
//    private WorldConfig[] worlds = new WorldConfig[]
//    {
//        new WorldConfig { worldType = WorldType.Add, worldKey = "Add", targetSceneName = "AddQuestions" },
//        new WorldConfig { worldType = WorldType.Minus, worldKey = "Minus", targetSceneName = "MinusQuestions" },
//        new WorldConfig { worldType = WorldType.Multiply, worldKey = "Multiply", targetSceneName = "MultiplyQuestions" },
//        new WorldConfig { worldType = WorldType.Divide, worldKey = "Divide", targetSceneName = "DivideQuestions" }
//    };

//    [Header("Currently Selected World")]
//    [SerializeField] private WorldType currentWorld = WorldType.Add;

//    [Header("Per-Level Sprites (index 0 = Level 1, index 9 = Level 10)")]
//    [SerializeField] private Sprite[] defaultSprites = new Sprite[10];
//    [SerializeField] private Sprite[] completedSprites = new Sprite[10];

//    private void Start()
//    {
//        LoadWorld(currentWorld);
//    }

//    /// <summary>
//    /// Call this function when the player selects a world tab/button.
//    /// Re-generates or re-configures the level buttons for the selected world.
//    /// </summary>
//    public void LoadWorld(WorldType selectedWorld)
//    {
//        currentWorld = selectedWorld;

//        // Find the active world config
//        WorldConfig activeConfig = GetConfigForWorld(selectedWorld);

//        // Clear existing spawned buttons (if switching worlds dynamically)
//        foreach (Transform child in buttonContainer)
//        {
//            Destroy(child.gameObject);
//        }

//        // Spawn buttons for the selected world
//        for (int i = 1; i <= totalLevels; i++)
//        {
//            // 1. Instantiate the prefab cleanly
//            GameObject buttonObj = Instantiate(levelButtonPrefab);

//            // 2. Set parent without keeping global world position offset
//            buttonObj.transform.SetParent(buttonContainer, false);

//            // 3. Reset local scale so UI doesn't shrink to 0
//            buttonObj.transform.localScale = Vector3.one;

//            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();

//            if (levelButton == null)
//            {
//                Debug.LogError($"Instantiated button for level {i} has no LevelButton component.");
//                continue;
//            }

//            bool isCompleted = IsLevelCompleted(activeConfig.worldKey, i);

//            int index = i - 1;
//            Sprite defaultSprite = index < defaultSprites.Length ? defaultSprites[index] : null;
//            Sprite completedSprite = index < completedSprites.Length ? completedSprites[index] : null;

//            levelButton.Setup(i, isCompleted, activeConfig.targetSceneName, defaultSprite, completedSprite);
//        }
//    }

//    private WorldConfig GetConfigForWorld(WorldType type)
//    {
//        foreach (var config in worlds)
//        {
//            if (config.worldType == type) return config;
//        }
//        return worlds[0]; // Default fallback
//    }

//    private bool IsLevelCompleted(string worldKey, int levelNumber)
//    {
//        return PlayerPrefs.GetInt(CompletionKey(worldKey, levelNumber), 0) == 1;
//    }

//    private string CompletionKey(string worldKey, int levelNumber)
//    {
//        return $"{worldKey}_Level{levelNumber}_Completed";
//    }

//    public static void MarkLevelCompleted(string worldKey, int levelNumber)
//    {
//        PlayerPrefs.SetInt($"{worldKey}_Level{levelNumber}_Completed", 1);
//        PlayerPrefs.Save();
//    }

//    ///// <summary>
//    ///// Call this from UI Button OnClick() events.
//    ///// Example: Passing 0 = Add, 1 = Minus, 2 = Multiply, 3 = Divide.
//    ///// </summary>
//    //public void SelectWorldByInt(int worldIndex)
//    //{
//    //    WorldType selected = (WorldType)worldIndex;
//    //    LoadWorld(selected);
//    //}

//    ///// <summary>
//    ///// Call this directly from script or Button events using string names ("Add", "Minus", etc.)
//    ///// </summary>
//    //public void SelectWorldByName(string worldName)
//    //{
//    //    if (System.Enum.TryParse(worldName, true, out WorldType selectedWorld))
//    //    {
//    //        LoadWorld(selectedWorld);
//    //    }
//    //    else
//    //    {
//    //        Debug.LogWarning($"World name '{worldName}' not recognized!");
//    //    }
//    //}
//}

