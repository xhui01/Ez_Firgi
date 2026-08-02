using System.Collections.Generic;

/// <summary>
/// A question generated from the KSSR syllabus units. Uses STRING answers/options
/// (not int like MathQuestion) because unit questions can be money ("RM19.80"),
/// time ("2 h 30 min"), ratios ("2:3"), fractions ("3/4"), or place-value words
/// ("Ten Thousands") - not just plain numbers.
/// </summary>
[System.Serializable]
public class UnitQuestion
{
    public string QuestionText;
    public string CorrectAnswer;
    public List<string> Options; // 4 options, shuffled, contains CorrectAnswer once
    public MathUnit Unit;
    public int Level;
}
