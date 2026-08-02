/// <summary>
/// The 8 KSSR Year 5 Mathematics units, matching the official DSKP topic list,
/// plus Mixed for the final combined level.
/// </summary>
public enum MathUnit
{
    WholeNumbers,               // Unit 1
    FractionsDecimalsPercent,   // Unit 2
    Money,                      // Unit 3
    Time,                       // Unit 4
    Measurement,                // Unit 5 - Length, Mass, Volume of Liquid
    Space,                      // Unit 6
    CoordinatesRatioProportion, // Unit 7
    DataHandling,               // Unit 8
    Mixed                       // Level 10 only - rotates through Units 1-8
}

public static class LevelUnitMap
{
    /// <summary>
    /// Level 1 -> Unit 1, Level 2 -> Unit 2, ... Level 8 -> Unit 8.
    /// Level 9 -> Unit 8 again (a second, harder pass on Data Handling).
    /// Level 10 -> Mixed (rotates a random unit per question).
    /// </summary>
    public static MathUnit GetUnitForLevel(int level)
    {
        switch (level)
        {
            case 1: return MathUnit.WholeNumbers;
            case 2: return MathUnit.FractionsDecimalsPercent;
            case 3: return MathUnit.Money;
            case 4: return MathUnit.Time;
            case 5: return MathUnit.Measurement;
            case 6: return MathUnit.Space;
            case 7: return MathUnit.CoordinatesRatioProportion;
            case 8: return MathUnit.DataHandling;
            case 9: return MathUnit.DataHandling; // harder second pass
            case 10: return MathUnit.Mixed;
            default: return MathUnit.WholeNumbers;
        }
    }

    /// <summary>
    /// Level 9 uses the same Unit as Level 8 but should feel harder -
    /// generators can check this to bump difficulty (bigger numbers, more data points, etc).
    /// </summary>
    public static bool IsHardTier(int level) => level == 9;
}
