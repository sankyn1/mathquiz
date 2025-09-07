// File: Services/SingleOpGenerator.cs
using MathQuiz.Models;

namespace MathQuiz.Services;

public class SingleOpGenerator : IQuestionGenerator
{
    private readonly Random _random = new();

    public string Mode => "Single-op";

    public bool SupportsLevel(int level) => level >= 1;

    public async Task<Question> GenerateQuestionAsync(int sessionId, int level, CancellationToken cancellationToken = default)
    {
        await Task.Yield(); // Make it async for consistency

        var (min, max) = GetRangeForLevel(level);
        var operation = GetRandomOperation(level);

        int a, b, correctAnswer;
        string expression;

        // Generate operands and calculate result
        switch (operation)
        {
            case '+':
                a = _random.Next(min, max + 1);
                b = _random.Next(min, max + 1);
                correctAnswer = a + b;
                expression = $"{a} + {b}";
                break;

            case '-':
                a = _random.Next(min + 5, max + 1);
                b = _random.Next(min, a); // Ensure non-negative result
                correctAnswer = a - b;
                expression = $"{a} - {b}";
                break;

            case '×':
                a = _random.Next(min, Math.Min(max, 12));
                b = _random.Next(min, Math.Min(max, 12));
                correctAnswer = a * b;
                expression = $"{a} × {b}";
                break;

            case '÷':
                b = _random.Next(Math.Max(min, 2), Math.Min(max, 12));
                correctAnswer = _random.Next(min, max / b + 1);
                a = b * correctAnswer; // Ensure integer division
                expression = $"{a} ÷ {b}";
                break;

            default:
                throw new InvalidOperationException($"Unknown operation: {operation}");
        }

        // Generate options with distractors
        var options = GenerateOptions(correctAnswer, operation, level);
        var correctIndex = ShuffleOptions(options, correctAnswer);

        return new Question(sessionId, expression, correctAnswer, options, correctIndex);
    }

    private (int min, int max) GetRangeForLevel(int level)
    {
        return level switch
        {
            1 => (1, 10),
            2 => (1, 20),
            3 => (2, 50),
            4 => (3, 100),
            _ => (5, Math.Min(500, 100 + (level - 4) * 50))
        };
    }

    private char GetRandomOperation(int level)
    {
        var operations = level switch
        {
            1 => new[] { '+', '-' },
            2 => new[] { '+', '-', '×' },
            _ => new[] { '+', '-', '×', '÷' }
        };

        return operations[_random.Next(operations.Length)];
    }

    private int[] GenerateOptions(int correctAnswer, char operation, int level)
    {
        var options = new HashSet<int> { correctAnswer };

        // Add distractors
        while (options.Count < 4)
        {
            int distractor = operation switch
            {
                '+' or '-' => GenerateAddSubDistractor(correctAnswer),
                '×' => GenerateMultiplyDistractor(correctAnswer),
                '÷' => GenerateDivideDistractor(correctAnswer),
                _ => correctAnswer + _random.Next(-5, 6)
            };

            if (distractor >= 0) // Avoid negative options for low levels
                options.Add(distractor);
        }

        return options.Take(4).ToArray();
    }

    private int GenerateAddSubDistractor(int correct)
    {
        return _random.Next(4) switch
        {
            0 => correct + 1,  // Off by one
            1 => correct - 1,  // Off by one
            2 => correct + _random.Next(2, 10), // Near range
            _ => Math.Max(0, correct - _random.Next(2, 10)) // Near range (non-negative)
        };
    }

    private int GenerateMultiplyDistractor(int correct)
    {
        return _random.Next(4) switch
        {
            0 => correct + _random.Next(1, Math.Max(2, correct / 4)), // Close but wrong
            1 => correct - _random.Next(1, Math.Max(2, correct / 4)), // Close but wrong
            2 => (int)(correct * 1.5), // Common mistake
            _ => correct / 2 // Half the answer
        };
    }

    private int GenerateDivideDistractor(int correct)
    {
        return _random.Next(4) switch
        {
            0 => correct + 1,
            1 => Math.Max(0, correct - 1),
            2 => correct * 2,
            _ => Math.Max(0, correct + _random.Next(-3, 4))
        };
    }

    private int ShuffleOptions(int[] options, int correctAnswer)
    {
        // Fisher-Yates shuffle
        for (int i = options.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        // Find the new index of the correct answer
        return Array.IndexOf(options, correctAnswer);
    }
}
