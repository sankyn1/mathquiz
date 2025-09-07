// File: Services/ISettingsService.cs
using MathQuiz.Models;

namespace MathQuiz.Services;

public interface ISettingsService
{
    Task<QuizSettings> GetSettingsAsync(int userId);
    Task SaveSettingsAsync(int userId, QuizSettings settings);
    Task<QuizSettings> GetDefaultSettingsAsync();
    void ApplyAccessibilitySettings(QuizSettings settings);
}
