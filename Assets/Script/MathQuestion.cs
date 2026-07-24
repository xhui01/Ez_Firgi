using System.Collections.Generic;

/// <summary>
/// Holds one generated question: the text, correct answer, and 4 shuffled options.
/// </summary>
[System.Serializable]
public class MathQuestion
{
    public string QuestionText;   // e.g. "456 + 378 = ?"
    public int CorrectAnswer;
    public List<int> Options;     // 4 options, already shuffled, contains CorrectAnswer once
    public MathOperation Operation;
    public int Level;
}

public enum MathOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}
