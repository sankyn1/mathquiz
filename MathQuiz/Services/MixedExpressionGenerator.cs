// File: Services/MixedExpressionGenerator.cs
using MathQuiz.Models;
using System.Text;

namespace MathQuiz.Services;

public class MixedExpressionGenerator : IQuestionGenerator
{
    private readonly Random _random = new();

    public string Mode => "Random Mix";

    public bool SupportsLevel(int level) => level >= 2;

    public async Task<Question> GenerateQuestionAsync(int sessionId, int level, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var opsCount = GetOperationsCountForLevel(level);
        var (min, max) = GetRangeForLevel(level);

        var expression = GenerateExpression(opsCount, min, max, level);
        var correctAnswer = EvaluateExpression(expression.expression);

        var options = GenerateOptions(correctAnswer, level);
        var correctIndex = ShuffleOptions(options, correctAnswer);

        return new Question(sessionId, expression.display, correctAnswer, options, correctIndex);
    }

    private int GetOperationsCountForLevel(int level)
    {
        return level switch
        {
            1 => 1,
            2 => _random.Next(1, 3), // 1-2 operations
            _ => _random.Next(2, 4)  // 2-3 operations
        };
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

    private (string expression, string display) GenerateExpression(int opsCount, int min, int max, int level)
    {
        var numbers = new List<int>();
        var operations = new List<char>();

        // Generate first number
        numbers.Add(_random.Next(min, max + 1));

        // Generate operations and subsequent numbers
        for (int i = 0; i < opsCount; i++)
        {
            var op = GetRandomOperation(level);
            operations.Add(op);

            int nextNumber;
            if (op == '÷' && level <= 2)
            {
                // Ensure integer division for low levels
                var dividend = numbers.Last();
                var divisors = GetDivisors(dividend, min, max);
                if (divisors.Any())
                    nextNumber = divisors[_random.Next(divisors.Count)];
                else
                    nextNumber = _random.Next(Math.Max(min, 2), Math.Min(max, 10));
            }
            else
            {
                nextNumber = _random.Next(min, max + 1);
            }

            numbers.Add(nextNumber);
        }

        // Build expression strings
        var expressionBuilder = new StringBuilder();
        var displayBuilder = new StringBuilder();

        for (int i = 0; i < numbers.Count; i++)
        {
            if (i > 0)
            {
                var op = operations[i - 1];
                expressionBuilder.Append($" {op} ");
                displayBuilder.Append($" {op} ");
            }

            expressionBuilder.Append(numbers[i]);
            displayBuilder.Append(numbers[i]);
        }

        return (expressionBuilder.ToString(), displayBuilder.ToString());
    }

    private List<int> GetDivisors(int number, int min, int max)
    {
        var divisors = new List<int>();
        for (int i = Math.Max(min, 2); i <= Math.Min(max, number); i++)
        {
            if (number % i == 0)
                divisors.Add(i);
        }
        return divisors;
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

    private int EvaluateExpression(string expression)
    {
        // Simple left-to-right evaluation (no operator precedence for mixed expressions)
        var tokens = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return 0;

        double result = int.Parse(tokens[0]);

        for (int i = 1; i < tokens.Length; i += 2)
        {
            if (i + 1 >= tokens.Length)
                break;

            var op = tokens[i][0];
            var operand = int.Parse(tokens[i + 1]);

            result = op switch
            {
                '+' => result + operand,
                '-' => result - operand,
                '×' => result * operand,
                '÷' => operand != 0 ? result / operand : result,
                _ => result
            };
        }

        return (int)Math.Round(result);
    }

    private int[] GenerateOptions(int correctAnswer, int level)
    {
        var options = new HashSet<int> { correctAnswer };
        var range = GetDistractorRange(correctAnswer, level);

        while (options.Count < 4)
        {
            int distractor = _random.Next(4) switch
            {
                0 => correctAnswer + 1,
                1 => Math.Max(0, correctAnswer - 1),
                2 => correctAnswer + _random.Next(2, range),
                _ => Math.Max(0, correctAnswer - _random.Next(2, range))
            };

            options.Add(distractor);
        }

        return options.Take(4).ToArray();
    }

    private int GetDistractorRange(int correctAnswer, int level)
    {
        var baseRange = Math.Max(5, correctAnswer / 5);
        return level switch
        {
            1 => Math.Min(baseRange, 5),
            2 => Math.Min(baseRange, 10),
            _ => Math.Min(baseRange, 20)
        };
    }

    private int ShuffleOptions(int[] options, int correctAnswer)
    {
        for (int i = options.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        return Array.IndexOf(options, correctAnswer);
    }
}
