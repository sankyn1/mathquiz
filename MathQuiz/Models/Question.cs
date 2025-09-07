// File: Models/Question.cs
using SQLite;

namespace MathQuiz.Models;

[Table("Questions")]
public class Question
{
    [PrimaryKey, AutoIncrement]
    public int QuestionId { get; set; }

    [NotNull]
    public int SessionId { get; set; }

    [NotNull]
    public string Expression { get; set; } = string.Empty;

    public int CorrectAnswer { get; set; }

    public int OptionA { get; set; }

    public int OptionB { get; set; }

    public int OptionC { get; set; }

    public int OptionD { get; set; }

    public int CorrectIndex { get; set; } // 0-3

    public Question() { }

    public Question(int sessionId, string expression, int correctAnswer, int[] options, int correctIndex)
    {
        SessionId = sessionId;
        Expression = expression;
        CorrectAnswer = correctAnswer;
        OptionA = options[0];
        OptionB = options[1];
        OptionC = options[2];
        OptionD = options[3];
        CorrectIndex = correctIndex;
    }

    public int[] GetOptions() => new[] { OptionA, OptionB, OptionC, OptionD };
}
