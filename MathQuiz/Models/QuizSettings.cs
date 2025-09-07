// File: Models/QuizSettings.cs
namespace MathQuiz.Models;

public class QuizSettings
{
    public string Mode { get; set; } = "Random Mix";
    public int QuestionsPerSession { get; set; } = 10;
    public bool TimerEnabled { get; set; } = false;
    public int TimerSeconds { get; set; } = 60;
    public bool SoundEnabled { get; set; } = true;
    public bool VibrationEnabled { get; set; } = true;
    public bool BiggerText { get; set; } = false;
    public bool HighContrast { get; set; } = false;
}
