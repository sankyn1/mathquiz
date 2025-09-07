// File: Models/Session.cs
using SQLite;

namespace MathQuiz.Models;

[Table("Sessions")]
public class Session
{
    [PrimaryKey, AutoIncrement]
    public int SessionId { get; set; }

    [NotNull]
    public int UserId { get; set; }

    [NotNull]
    public string Mode { get; set; } = string.Empty;

    [NotNull]
    public string StartedAt { get; set; } = DateTime.UtcNow.ToString("O");

    public string? EndedAt { get; set; }

    public int TotalQuestions { get; set; }

    public int Score { get; set; }

    public double Accuracy { get; set; }

    public int MaxStreak { get; set; }

    public int HighestLevel { get; set; }

    public Session() { }

    public Session(int userId, string mode, int totalQuestions)
    {
        UserId = userId;
        Mode = mode;
        TotalQuestions = totalQuestions;
        StartedAt = DateTime.UtcNow.ToString("O");
    }

    public void EndSession(int score, double accuracy, int maxStreak, int highestLevel)
    {
        Score = score;
        Accuracy = accuracy;
        MaxStreak = maxStreak;
        HighestLevel = highestLevel;
        EndedAt = DateTime.UtcNow.ToString("O");
    }
}
