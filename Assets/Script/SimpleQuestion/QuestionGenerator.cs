using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates random math questions with multiple-choice options.
/// Difficulty (number range) scales automatically with the level (1-10).
/// Attach nothing - this is a plain C# static-ish helper you call from GameManager.
/// </summary>
public class QuestionGenerator
{
    private readonly System.Random _rng = new System.Random();

    /// <summary>
    /// Generate one question for the given operation and level (1-10).
    /// </summary>
    public MathQuestion Generate(MathOperation operation, int level)
    {
        level = Mathf.Clamp(level, 1, 10);
        (int min, int max) = GetNumberRange(level);

        int a, b, correctAnswer;
        string questionText;

        switch (operation)
        {
            case MathOperation.Add:
                a = RandomInt(min, max);
                b = RandomInt(min, max);
                correctAnswer = a + b;
                questionText = $"{a} + {b} = ?";
                break;

            case MathOperation.Subtract:
                a = RandomInt(min, max);
                b = RandomInt(min, max);
                // Ensure no negative results for younger levels
                if (a < b) (a, b) = (b, a);
                correctAnswer = a - b;
                questionText = $"{a} - {b} = ?";
                break;

            case MathOperation.Multiply:
                // Multiplication needs smaller ranges or numbers explode fast
                int multMin = Mathf.Max(2, min / 10 == 0 ? 2 : min / 10);
                int multMax = Mathf.Clamp(max / 10, multMin + 1, 12 + level); // scales 2..~22
                a = RandomInt(multMin, multMax);
                b = RandomInt(multMin, multMax);
                correctAnswer = a * b;
                questionText = $"{a} × {b} = ?";
                break;

            case MathOperation.Divide:
                // Build a clean division: pick divisor + quotient, multiply to get dividend
                int divisor = RandomInt(2, 4 + level);       // grows with level
                int quotient = RandomInt(2, 4 + level);
                int dividend = divisor * quotient;
                correctAnswer = quotient;
                questionText = $"{dividend} ÷ {divisor} = ?";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }

        List<int> options = BuildOptions(correctAnswer, level);

        return new MathQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options,
            Operation = operation,
            Level = level
        };
    }

    /// <summary>
    /// Number range grows with level so Tahap 1 is easy and Tahap 10 is hard.
    /// Tune these to match your syllabus (e.g. Tahun 5 add/subtract up to 3 digits).
    /// </summary>
    private (int min, int max) GetNumberRange(int level)
    {
        // Level 1 -> roughly 1-50, Level 10 -> roughly 1-999
        int max = Mathf.RoundToInt(Mathf.Lerp(50, 999, (level - 1) / 9f));
        int min = 1;
        return (min, max);
    }

    private int RandomInt(int min, int maxInclusive) => _rng.Next(min, maxInclusive + 1);

    /// <summary>
    /// Builds 4 shuffled options: 1 correct + 3 plausible distractors
    /// (close to correct answer, no duplicates, no negatives).
    /// </summary>
    private List<int> BuildOptions(int correctAnswer, int level)
    {
        var options = new HashSet<int> { correctAnswer };
        int spread = Mathf.Max(2, 5 + level); // wrong answers get "closer" and trickier at higher levels' inverse, tweak as needed

        while (options.Count < 4)
        {
            int offset = RandomInt(1, spread) * (_rng.Next(2) == 0 ? 1 : -1);
            int candidate = correctAnswer + offset;
            if (candidate >= 0 && !options.Contains(candidate))
            {
                options.Add(candidate);
            }
        }

        // Shuffle so the correct answer isn't always in the same slot
        return options.OrderBy(_ => _rng.Next()).ToList();
    }
}
