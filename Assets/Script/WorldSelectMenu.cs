using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSelectMenu : MonoBehaviour
{
    // 1. Click "DUNIA TAMBAH" (Add World)
    public void SelectAddWorld()
    {
        WorldSelectionState.CurrentSelectedWorld = LevelSelectManager.WorldType.Add;
        SceneManager.LoadScene("AddWorld"); // Opens Add World Scene
    }

    // 2. Click "DUNIA TOLAK" (Minus World)
    public void SelectMinusWorld()
    {
        WorldSelectionState.CurrentSelectedWorld = LevelSelectManager.WorldType.Minus;
        SceneManager.LoadScene("MinusWorld"); // Opens Minus World Scene
    }

    // 3. Click "DUNIA DARAB" (Multiply World)
    public void SelectMultiplyWorld()
    {
        WorldSelectionState.CurrentSelectedWorld = LevelSelectManager.WorldType.Multiply;
        SceneManager.LoadScene("MultiplyWorld"); // Opens Multiply World Scene
    }

    // 4. Click "DUNIA BAHAGI" (Divide World)
    public void SelectDivideWorld()
    {
        WorldSelectionState.CurrentSelectedWorld = LevelSelectManager.WorldType.Divide;
        SceneManager.LoadScene("DivideWorld"); // Opens Divide World Scene
    }
}