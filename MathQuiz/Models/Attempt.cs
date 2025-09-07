// File: Models/Attempt.cs
using SQLite;

namespace MathQuiz.Models;

[Table("Attempts")]
public class Attempt
{
    [PrimaryKey, AutoIncrement]
    public int AttemptId { get; set; }

    [NotNull]
    public int QuestionId { get; set; }

    public int SelectedIndex { get; set; } // 0-3

    public int IsCorrect { get; set; } // 0 or 1 (SQLite doesn't have bool)

    public int AnswerTimeMs { get; set; }

    public bool Correct => IsCorrect == 1;

    public Attempt() { }

    public Attempt(int questionId, int selectedIndex, bool isCorrect, int answerTimeMs)
    {
        QuestionId = questionId;
        SelectedIndex = selectedIndex;
        IsCorrect = isCorrect ? 1 : 0;
        AnswerTimeMs = answerTimeMs;
    }
}
