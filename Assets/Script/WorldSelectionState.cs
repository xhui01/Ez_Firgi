public static class WorldSelectionState
{
    // Tracks which world is selected across scene transitions.
    // Defaults to Add world.
    public static LevelSelectManager.WorldType CurrentSelectedWorld = LevelSelectManager.WorldType.Add;
}