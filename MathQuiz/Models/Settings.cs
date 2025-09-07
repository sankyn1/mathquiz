// File: Models/Settings.cs
using SQLite;

namespace MathQuiz.Models;

[Table("Settings")]
public class Settings
{
    [PrimaryKey]
    public int UserId { get; set; }

    [NotNull]
    public string Mode { get; set; } = "Random Mix";

    public int QuestionsPerSession { get; set; } = 10;

    public int TimerSeconds { get; set; } = 0; // 0 = disabled

    public int SoundEnabled { get; set; } = 1;

    public int VibrationEnabled { get; set; } = 1;

    public string? AccessibilitySettings { get; set; }

    // Helper properties
    public bool Sound => SoundEnabled == 1;
    public bool Vibration => VibrationEnabled == 1;
    public bool TimerEnabled => TimerSeconds > 0;

    public Settings() { }

    public Settings(int userId)
    {
        UserId = userId;
    }
}
