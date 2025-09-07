// File: Models/GameState.cs
namespace MathQuiz.Models;

public class GameState
{
    public int CurrentQuestionIndex { get; set; }
    public int Score { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int ConsecutiveWrong { get; set; }
    public DateTime SessionStart { get; set; } = DateTime.UtcNow;
    public List<Question> Questions { get; set; } = new();
    public List<Attempt> Attempts { get; set; } = new();

    public double Accuracy =>
        (CorrectAnswers + WrongAnswers) == 0 ? 0 :
        (double)CorrectAnswers / (CorrectAnswers + WrongAnswers) * 100;

    public int HighestLevel { get; set; } = 1;

    public void UpdateLevel()
    {
        HighestLevel = Math.Max(HighestLevel, CurrentLevel);
    }
}
