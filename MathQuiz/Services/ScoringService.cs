// File: Services/ScoringService.cs
namespace MathQuiz.Services;

public class ScoringService : IScoringService
{
    private const int BaseScore = 10;
    private const int StreakBonusMultiplier = 2;
    private const int StreakBonusThreshold = 3;

    public int CalculateScore(int currentScore, int streak, bool isCorrect, int answerTimeMs = 0, bool timerEnabled = false)
    {
        if (!isCorrect)
            return currentScore; // No points for wrong answers

        var score = BaseScore;

        // Add streak bonus
        score += CalculateStreakBonus(streak);

        // Add time bonus if timer is enabled
        if (timerEnabled && answerTimeMs > 0)
        {
            var timeBonus = CalculateTimeBonus(answerTimeMs, 10000); // Assume 10 seconds per question
            score += timeBonus;
        }

        return currentScore + score;
    }

    public int CalculateStreakBonus(int streak)
    {
        return StreakBonusMultiplier * (streak / StreakBonusThreshold);
    }

    public int CalculateTimeBonus(int answerTimeMs, int totalTimeMs)
    {
        if (answerTimeMs <= 0 || totalTimeMs <= 0)
            return 0;

        var timeRatio = (double)answerTimeMs / totalTimeMs;

        return timeRatio switch
        {
            < 0.25 => 5, // Super fast
            < 0.5 => 3,  // Fast
            < 0.75 => 1, // Decent
            _ => 0       // Slow
        };
    }
}
