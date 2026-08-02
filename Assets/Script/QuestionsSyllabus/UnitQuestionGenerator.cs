using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates Year 5 KSSR syllabus questions. Which UNIT is used depends on the level
/// (1-10, via LevelUnitMap). Which OPERATION is used within that unit depends on
/// which world you're in (Add/Subtract/Multiply/Divide) - so e.g. Level 3 (Money) in
/// AddWorld gives "RM12.50 + RM7.30", while Level 3 in DivideWorld gives
/// "total cost RM50, 5 items, find price per item".
/// </summary>
public class UnitQuestionGenerator
{
    private readonly System.Random _rng = new System.Random();

    public UnitQuestion Generate(MathOperation operation, int level)
    {
        MathUnit unit = LevelUnitMap.GetUnitForLevel(level);
        bool hard = LevelUnitMap.IsHardTier(level);

        // Level 10: pick a random real unit (never Mixed) so each question
        // still comes from exactly one topic, using the world's operation throughout.
        if (unit == MathUnit.Mixed)
        {
            Array allUnits = Enum.GetValues(typeof(MathUnit));
            MathUnit picked;
            do { picked = (MathUnit)allUnits.GetValue(_rng.Next(allUnits.Length)); }
            while (picked == MathUnit.Mixed);
            unit = picked;
        }

        UnitQuestion q = unit switch
        {
            MathUnit.WholeNumbers => GenerateWholeNumbers(operation),
            MathUnit.FractionsDecimalsPercent => GenerateFractionsDecimals(operation),
            MathUnit.Money => GenerateMoney(operation),
            MathUnit.Time => GenerateTime(operation),
            MathUnit.Measurement => GenerateMeasurement(operation),
            MathUnit.Space => GenerateSpace(operation),
            MathUnit.CoordinatesRatioProportion => GenerateCoordinatesRatioProportion(operation),
            MathUnit.DataHandling => GenerateDataHandling(operation, hard),
            _ => GenerateWholeNumbers(operation)
        };

        q.Level = level;
        return q;
    }

    private int RandomInt(int min, int maxInclusive) => _rng.Next(min, maxInclusive + 1);

    // ---------------- UNIT 1: Whole Numbers ----------------
    private UnitQuestion GenerateWholeNumbers(MathOperation operation)
    {
        int a, b, correct;
        string symbol;

        switch (operation)
        {
            case MathOperation.Add:
                a = RandomInt(10000, 500000);
                b = RandomInt(10000, 500000);
                correct = a + b;
                symbol = "+";
                break;
            case MathOperation.Subtract:
                a = RandomInt(50000, 999999);
                b = RandomInt(10000, a);
                correct = a - b;
                symbol = "-";
                break;
            case MathOperation.Multiply:
                a = RandomInt(100, 999);
                b = RandomInt(2, 12);
                correct = a * b;
                symbol = "×";
                break;
            default: // Divide
                int divisor = RandomInt(2, 12);
                int quotient = RandomInt(100, 999);
                a = divisor * quotient;
                b = divisor;
                correct = quotient;
                symbol = "÷";
                break;
        }

        string questionText = $"{FormatThousands(a)} {symbol} {FormatThousands(b)} = ?";
        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correct.ToString(),
            Options = BuildNumericOptions(correct, Mathf.Max(5, correct / 20)),
            Unit = MathUnit.WholeNumbers
        };
    }

    // ---------------- UNIT 2: Fractions / Decimals ----------------
    private UnitQuestion GenerateFractionsDecimals(MathOperation operation)
    {
        if (operation == MathOperation.Add || operation == MathOperation.Subtract)
        {
            int denominator = RandomInt(4, 12);
            int numA, numB, correctNum;
            string symbol;

            if (operation == MathOperation.Add)
            {
                numA = RandomInt(1, denominator - 2);
                numB = RandomInt(1, denominator - numA);
                correctNum = numA + numB;
                symbol = "+";
            }
            else
            {
                numA = RandomInt(2, denominator - 1);
                numB = RandomInt(1, numA - 1);
                correctNum = numA - numB;
                symbol = "-";
            }

            string questionText = $"{numA}/{denominator} {symbol} {numB}/{denominator} = ?";
            string correctAnswer = $"{correctNum}/{denominator}";

            var options = new HashSet<string> { correctAnswer };
            while (options.Count < 4)
            {
                int offset = RandomInt(1, 3) * (_rng.Next(2) == 0 ? 1 : -1);
                int candidate = correctNum + offset;
                if (candidate > 0 && candidate < denominator * 2)
                    options.Add($"{candidate}/{denominator}");
            }

            return new UnitQuestion
            {
                QuestionText = questionText,
                CorrectAnswer = correctAnswer,
                Options = options.OrderBy(_ => _rng.Next()).ToList(),
                Unit = MathUnit.FractionsDecimalsPercent
            };
        }
        else
        {
            // Multiply / Divide: decimal x whole number
            double decimalValue = RandomInt(10, 500) / 10.0;
            int whole = RandomInt(2, 9);
            double correct;
            string questionText;

            if (operation == MathOperation.Multiply)
            {
                correct = Math.Round(decimalValue * whole, 1);
                questionText = $"{decimalValue:0.0} × {whole} = ?";
            }
            else
            {
                // Build a clean division: correct x whole = decimalValue-ish dividend
                correct = decimalValue;
                double dividend = Math.Round(correct * whole, 1);
                questionText = $"{dividend:0.0} ÷ {whole} = ?";
            }

            string correctAnswer = correct.ToString("0.0");
            var options = new HashSet<string> { correctAnswer };
            while (options.Count < 4)
            {
                double offset = RandomInt(1, 5) / 10.0 * (_rng.Next(2) == 0 ? 1 : -1);
                double candidate = Math.Round(correct + offset, 1);
                if (candidate > 0) options.Add(candidate.ToString("0.0"));
            }

            return new UnitQuestion
            {
                QuestionText = questionText,
                CorrectAnswer = correctAnswer,
                Options = options.OrderBy(_ => _rng.Next()).ToList(),
                Unit = MathUnit.FractionsDecimalsPercent
            };
        }
    }

    // ---------------- UNIT 3: Money ----------------
    private UnitQuestion GenerateMoney(MathOperation operation)
    {
        string questionText;
        decimal correct;

        switch (operation)
        {
            case MathOperation.Add:
            {
                decimal a = RandomInt(100, 9999) / 100m;
                decimal b = RandomInt(100, 9999) / 100m;
                correct = a + b;
                questionText = $"RM{a:0.00} + RM{b:0.00} = ?";
                break;
            }
            case MathOperation.Subtract:
            {
                decimal price = RandomInt(500, 4500) / 100m;
                decimal[] notes = { 5m, 10m, 20m, 50m, 100m };
                decimal paid = notes.First(n => n > price + 1);
                correct = paid - price;
                questionText = $"An item costs RM{price:0.00}. You pay with RM{paid:0.00}. What is your change?";
                break;
            }
            case MathOperation.Multiply:
            {
                decimal unitPrice = RandomInt(100, 2000) / 100m;
                int qty = RandomInt(2, 9);
                correct = unitPrice * qty;
                questionText = $"One item costs RM{unitPrice:0.00}. What is the total cost for {qty} items?";
                break;
            }
            default: // Divide
            {
                int qty = RandomInt(2, 9);
                decimal unitPrice = RandomInt(100, 2000) / 100m;
                decimal total = Math.Round(unitPrice * qty, 2);
                correct = unitPrice;
                questionText = $"{qty} items cost a total of RM{total:0.00}. What is the price of ONE item?";
                break;
            }
        }

        string correctAnswer = $"RM{correct:0.00}";
        var options = new HashSet<string> { correctAnswer };
        while (options.Count < 4)
        {
            decimal offset = RandomInt(10, 300) / 100m * (_rng.Next(2) == 0 ? 1 : -1);
            decimal candidate = Math.Round(correct + offset, 2);
            if (candidate > 0) options.Add($"RM{candidate:0.00}");
        }

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options.OrderBy(_ => _rng.Next()).ToList(),
            Unit = MathUnit.Money
        };
    }

    // ---------------- UNIT 4: Time ----------------
    private UnitQuestion GenerateTime(MathOperation operation)
    {
        string questionText, correctAnswer;
        var options = new HashSet<string>();

        if (operation == MathOperation.Subtract)
        {
            // Duration between two times (matches your original design)
            int startHour = RandomInt(1, 11);
            int startMin = RandomInt(0, 11) * 5;
            int durationMin = RandomInt(2, 24) * 5;
            int totalEndMin = startHour * 60 + startMin + durationMin;
            int endHour = (totalEndMin / 60) % 12; if (endHour == 0) endHour = 12;
            int endMin = totalEndMin % 60;

            questionText = $"A class starts at {startHour}:{startMin:00} pm and ends at {endHour}:{endMin:00} pm. How long is the class?";
            int ch = durationMin / 60, cm = durationMin % 60;
            correctAnswer = ch > 0 ? $"{ch} h {cm} min" : $"{cm} min";

            options.Add(correctAnswer);
            while (options.Count < 4)
            {
                int off = RandomInt(5, 20) * (_rng.Next(2) == 0 ? 1 : -1);
                int cand = Mathf.Max(5, durationMin + off);
                int h = cand / 60, m = cand % 60;
                options.Add(h > 0 ? $"{h} h {m} min" : $"{m} min");
            }
        }
        else if (operation == MathOperation.Add)
        {
            // Add a duration onto a start time -> find end time
            int startHour = RandomInt(1, 10);
            int startMin = RandomInt(0, 11) * 5;
            int durationMin = RandomInt(2, 20) * 5;
            int totalEndMin = startHour * 60 + startMin + durationMin;
            int endHour = (totalEndMin / 60) % 12; if (endHour == 0) endHour = 12;
            int endMin = totalEndMin % 60;

            questionText = $"A bus leaves at {startHour}:{startMin:00} pm. The journey takes {durationMin} minutes. What time does it arrive?";
            correctAnswer = $"{endHour}:{endMin:00} pm";

            options.Add(correctAnswer);
            while (options.Count < 4)
            {
                int offMin = RandomInt(5, 20) * (_rng.Next(2) == 0 ? 1 : -1);
                int candTotal = totalEndMin + offMin;
                int h = (candTotal / 60) % 12; if (h == 0) h = 12;
                int m = ((candTotal % 60) + 60) % 60;
                options.Add($"{h}:{m:00} pm");
            }
        }
        else if (operation == MathOperation.Multiply)
        {
            int perSessionMin = RandomInt(2, 12) * 5;
            int sessions = RandomInt(2, 6);
            int totalMin = perSessionMin * sessions;

            questionText = $"A piano lesson lasts {perSessionMin} minutes. How long are {sessions} lessons in total?";
            int ch = totalMin / 60, cm = totalMin % 60;
            correctAnswer = ch > 0 ? $"{ch} h {cm} min" : $"{cm} min";

            options.Add(correctAnswer);
            while (options.Count < 4)
            {
                int off = RandomInt(5, 20) * (_rng.Next(2) == 0 ? 1 : -1);
                int cand = Mathf.Max(5, totalMin + off);
                int h = cand / 60, m = cand % 60;
                options.Add(h > 0 ? $"{h} h {m} min" : $"{m} min");
            }
        }
        else // Divide
        {
            int sessions = RandomInt(2, 6);
            int perSessionMin = RandomInt(2, 12) * 5;
            int totalMin = perSessionMin * sessions;

            int th = totalMin / 60, tm = totalMin % 60;
            string totalText = th > 0 ? $"{th} h {tm} min" : $"{tm} min";

            questionText = $"A total of {totalText} is spent equally across {sessions} training sessions. How long is EACH session?";
            correctAnswer = $"{perSessionMin} min";

            options.Add(correctAnswer);
            while (options.Count < 4)
            {
                int off = RandomInt(5, 15) * (_rng.Next(2) == 0 ? 1 : -1);
                int cand = Mathf.Max(5, perSessionMin + off);
                options.Add($"{cand} min");
            }
        }

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options.OrderBy(_ => _rng.Next()).ToList(),
            Unit = MathUnit.Time
        };
    }

    // ---------------- UNIT 5: Length, Mass, Volume ----------------
    private UnitQuestion GenerateMeasurement(MathOperation operation)
    {
        string[] pairs = { "km-m", "kg-g", "l-ml" };
        string chosen = pairs[RandomInt(0, pairs.Length - 1)];
        string bigUnit = chosen switch { "km-m" => "km", "kg-g" => "kg", _ => "l" };
        string smallUnit = chosen switch { "km-m" => "m", "kg-g" => "g", _ => "ml" };

        string questionText, correctAnswer;
        var options = new HashSet<string>();

        if (operation == MathOperation.Multiply)
        {
            int wholeUnits = RandomInt(2, 15);
            int correct = wholeUnits * 1000;
            questionText = $"Convert {wholeUnits} {bigUnit} to {smallUnit}.";
            correctAnswer = $"{correct} {smallUnit}";
            foreach (var s in BuildRawNumericOptions(correct, 500)) options.Add($"{s} {smallUnit}");
        }
        else if (operation == MathOperation.Divide)
        {
            int wholeUnits = RandomInt(2, 15);
            int smallValue = wholeUnits * 1000;
            questionText = $"Convert {smallValue} {smallUnit} to {bigUnit}.";
            correctAnswer = $"{wholeUnits} {bigUnit}";
            foreach (var s in BuildRawNumericOptions(wholeUnits, 3)) options.Add($"{s} {bigUnit}");
        }
        else
        {
            int a = RandomInt(2, 40);
            int b = RandomInt(2, 40);
            int correct;
            string symbol;

            if (operation == MathOperation.Add) { correct = a + b; symbol = "+"; }
            else { if (a < b) (a, b) = (b, a); correct = a - b; symbol = "-"; }

            questionText = $"{a} {smallUnit} {symbol} {b} {smallUnit} = ?";
            correctAnswer = $"{correct} {smallUnit}";
            foreach (var s in BuildRawNumericOptions(correct, Mathf.Max(3, correct / 10))) options.Add($"{s} {smallUnit}");
        }

        options.Add(correctAnswer);

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options.Take(4).OrderBy(_ => _rng.Next()).ToList(),
            Unit = MathUnit.Measurement
        };
    }

    // ---------------- UNIT 6: Space ----------------
    private UnitQuestion GenerateSpace(MathOperation operation)
    {
        int length = RandomInt(6, 30);
        int width = RandomInt(3, length - 1);
        string questionText, correctAnswer;

        switch (operation)
        {
            case MathOperation.Add:
            {
                int correct = 2 * (length + width);
                questionText = $"A rectangle has a length of {length} cm and a width of {width} cm. What is its perimeter?";
                correctAnswer = $"{correct} cm";
                return BuildSpaceQuestion(questionText, correctAnswer, correct, "cm");
            }
            case MathOperation.Subtract:
            {
                int perimeter = 2 * (length + width);
                int correct = width;
                questionText = $"A rectangle's perimeter is {perimeter} cm. Its length is {length} cm. What is its width?";
                correctAnswer = $"{correct} cm";
                return BuildSpaceQuestion(questionText, correctAnswer, correct, "cm");
            }
            case MathOperation.Multiply:
            {
                int correct = length * width;
                questionText = $"A rectangle has a length of {length} cm and a width of {width} cm. What is its area?";
                correctAnswer = $"{correct} cm²";
                return BuildSpaceQuestion(questionText, correctAnswer, correct, "cm²");
            }
            default: // Divide
            {
                int area = length * width;
                int correct = width;
                questionText = $"A rectangle's area is {area} cm². Its length is {length} cm. What is its width?";
                correctAnswer = $"{correct} cm";
                return BuildSpaceQuestion(questionText, correctAnswer, correct, "cm");
            }
        }
    }

    private UnitQuestion BuildSpaceQuestion(string questionText, string correctAnswer, int correctValue, string suffix)
    {
        var options = new HashSet<string> { correctAnswer };
        foreach (var s in BuildRawNumericOptions(correctValue, Mathf.Max(3, correctValue / 10)))
            options.Add($"{s} {suffix}");

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options.Take(4).OrderBy(_ => _rng.Next()).ToList(),
            Unit = MathUnit.Space
        };
    }

    // ---------------- UNIT 7: Coordinates, Ratio, Proportion ----------------
    private UnitQuestion GenerateCoordinatesRatioProportion(MathOperation operation)
    {
        string questionText, correctAnswer;
        var options = new HashSet<string>();

        switch (operation)
        {
            case MathOperation.Add:
            {
                int a = RandomInt(1, 8), b = RandomInt(1, 8);
                int correct = a + b;
                questionText = $"In the ratio {a}:{b}, what is the total number of parts?";
                correctAnswer = correct.ToString();
                foreach (var s in BuildRawNumericOptions(correct, 3)) options.Add(s);
                break;
            }
            case MathOperation.Subtract:
            {
                int x = RandomInt(1, 10);
                int yA = RandomInt(1, 15), yB = yA + RandomInt(2, 10);
                int correct = yB - yA;
                questionText = $"Point A is at ({x}, {yA}). Point B is at ({x}, {yB}). Find the distance AB.";
                correctAnswer = correct.ToString();
                foreach (var s in BuildRawNumericOptions(correct, 3)) options.Add(s);
                break;
            }
            case MathOperation.Multiply:
            {
                int simpleA = RandomInt(1, 6), simpleB = RandomInt(1, 6);
                while (GreatestCommonDivisor(simpleA, simpleB) != 1) simpleB = RandomInt(1, 6);
                int factor = RandomInt(2, 5);
                int a = simpleA * factor, b = simpleB * factor;

                questionText = $"Write an equivalent ratio for {simpleA}:{simpleB} using a scale factor of {factor}.";
                correctAnswer = $"{a}:{b}";
                options.Add(correctAnswer);
                while (options.Count < 4)
                {
                    int da = Mathf.Max(1, a + RandomInt(-3, 3));
                    int db = Mathf.Max(1, b + RandomInt(-3, 3));
                    options.Add($"{da}:{db}");
                }
                break;
            }
            default: // Divide
            {
                int factor = RandomInt(2, 6);
                int simpleA = RandomInt(1, 6), simpleB = RandomInt(1, 6);
                while (GreatestCommonDivisor(simpleA, simpleB) != 1) simpleB = RandomInt(1, 6);
                int a = simpleA * factor, b = simpleB * factor;

                questionText = $"Simplify the ratio {a}:{b}.";
                correctAnswer = $"{simpleA}:{simpleB}";
                options.Add(correctAnswer);
                while (options.Count < 4)
                {
                    int da = Mathf.Max(1, simpleA + RandomInt(-2, 2));
                    int db = Mathf.Max(1, simpleB + RandomInt(-2, 2));
                    options.Add($"{da}:{db}");
                }
                break;
            }
        }

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = options.Take(4).OrderBy(_ => _rng.Next()).ToList(),
            Unit = MathUnit.CoordinatesRatioProportion
        };
    }

    // ---------------- UNIT 8: Data Handling ----------------
    private UnitQuestion GenerateDataHandling(MathOperation operation, bool hard)
    {
        int count = hard ? RandomInt(6, 8) : RandomInt(4, 5);
        var data = new List<int>();
        for (int i = 0; i < count; i++) data.Add(RandomInt(2, 20));

        string questionText, correctAnswer;
        int correct;

        switch (operation)
        {
            case MathOperation.Add:
                correct = data.Sum();
                questionText = $"Find the total of: {string.Join(", ", data)}";
                correctAnswer = correct.ToString();
                break;

            case MathOperation.Subtract:
                correct = data.Max() - data.Min();
                questionText = $"Find the range (highest - lowest) of: {string.Join(", ", data)}";
                correctAnswer = correct.ToString();
                break;

            case MathOperation.Multiply:
            {
                int value = RandomInt(2, 15);
                int frequency = RandomInt(3, 9);
                correct = value * frequency;
                questionText = $"{frequency} students each scored {value} marks. What is the TOTAL marks scored?";
                correctAnswer = correct.ToString();
                break;
            }

            default: // Divide (mean)
                int sum = data.Sum();
                while (sum % count != 0) { data[RandomInt(0, count - 1)]++; sum = data.Sum(); }
                correct = sum / count;
                questionText = $"Find the average (mean) of: {string.Join(", ", data)}";
                correctAnswer = correct.ToString();
                break;
        }

        return new UnitQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Options = BuildNumericOptions(correct, Mathf.Max(2, correct / 6)),
            Unit = MathUnit.DataHandling
        };
    }

    // ---------------- Shared helpers ----------------

    private List<string> BuildNumericOptions(int correctAnswer, int spread)
    {
        return BuildRawNumericOptions(correctAnswer, spread).ToList();
    }

    private List<string> BuildRawNumericOptions(int correctAnswer, int spread)
    {
        var options = new HashSet<int> { correctAnswer };
        spread = Mathf.Max(2, spread);

        while (options.Count < 4)
        {
            int offset = RandomInt(1, spread) * (_rng.Next(2) == 0 ? 1 : -1);
            int candidate = correctAnswer + offset;
            if (candidate >= 0 && !options.Contains(candidate))
                options.Add(candidate);
        }

        return options.OrderBy(_ => _rng.Next()).Select(o => o.ToString()).ToList();
    }

    private string FormatThousands(int number) => number.ToString("N0").Replace(",", " ");

    private int GreatestCommonDivisor(int a, int b) => b == 0 ? a : GreatestCommonDivisor(b, a % b);
}
