// File: Services/SettingsService.cs
using MathQuiz.Models;

namespace MathQuiz.Services;

public class SettingsService : ISettingsService
{
    private readonly IPersistenceService _persistence;

    public SettingsService(IPersistenceService persistence)
    {
        _persistence = persistence;
    }

    public async Task<QuizSettings> GetSettingsAsync(int userId)
    {
        var dbSettings = await _persistence.GetSettingsAsync(userId);

        if (dbSettings == null)
            return await GetDefaultSettingsAsync();

        return new QuizSettings
        {
            Mode = dbSettings.Mode,
            QuestionsPerSession = dbSettings.QuestionsPerSession,
            TimerEnabled = dbSettings.TimerEnabled,
            TimerSeconds = dbSettings.TimerSeconds,
            SoundEnabled = dbSettings.Sound,
            VibrationEnabled = dbSettings.Vibration,
            BiggerText = ParseAccessibilitySetting(dbSettings.AccessibilitySettings, "BiggerText"),
            HighContrast = ParseAccessibilitySetting(dbSettings.AccessibilitySettings, "HighContrast")
        };
    }

    public async Task SaveSettingsAsync(int userId, QuizSettings settings)
    {
        var dbSettings = new Settings(userId)
        {
            Mode = settings.Mode,
            QuestionsPerSession = settings.QuestionsPerSession,
            TimerSeconds = settings.TimerEnabled ? settings.TimerSeconds : 0,
            SoundEnabled = settings.SoundEnabled ? 1 : 0,
            VibrationEnabled = settings.VibrationEnabled ? 1 : 0,
            AccessibilitySettings = SerializeAccessibilitySettings(settings)
        };

        await _persistence.SaveSettingsAsync(dbSettings);
        ApplyAccessibilitySettings(settings);
    }

    public async Task<QuizSettings> GetDefaultSettingsAsync()
    {
        return new QuizSettings
        {
            Mode = "Random Mix",
            QuestionsPerSession = 10,
            TimerEnabled = false,
            TimerSeconds = 60,
            SoundEnabled = true,
            VibrationEnabled = true,
            BiggerText = false,
            HighContrast = false
        };
    }

    public void ApplyAccessibilitySettings(QuizSettings settings)
    {
        // Apply theme changes based on accessibility settings
        if (settings.HighContrast)
        {
            Application.Current?.Resources.MergedDictionaries.Add(
                new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Styles/HighContrastTheme.xaml") });
        }

        // Text scaling would be handled by the OS, but we can provide hints
        if (settings.BiggerText)
        {
            // This would be implemented with dynamic resource updates
        }
    }

    private bool ParseAccessibilitySetting(string? json, string key)
    {
        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            return json.Contains($"\"{key}\":true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private string SerializeAccessibilitySettings(QuizSettings settings)
    {
        return $"{{\"BiggerText\":{settings.BiggerText.ToString().ToLower()},\"HighContrast\":{settings.HighContrast.ToString().ToLower()}}}";
    }
}
