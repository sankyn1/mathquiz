// File: Services/IScoringService.cs
namespace MathQuiz.Services;

public interface IScoringService
{
    int CalculateScore(int currentScore, int streak, bool isCorrect, int answerTimeMs = 0, bool timerEnabled = false);
    int CalculateStreakBonus(int streak);
    int CalculateTimeBonus(int answerTimeMs, int totalTimeMs);
}
