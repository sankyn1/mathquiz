// File: ViewModels/SettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;

namespace MathQuiz.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private QuizSettings _settings = new();

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private List<string> _modeOptions = new() { "Single-op", "Random Mix" };

    [ObservableProperty]
    private List<int> _questionCountOptions = new() { 5, 10, 15, 20, 25, 30 };

    [ObservableProperty]
    private List<int> _timerOptions = new() { 30, 60, 90, 120, 180 };

    private int _userId = 1; // Will be set from navigation parameter

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        Title = "Settings";
    }

    public async Task InitializeAsync(int userId)
    {
        _userId = userId;
        await LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;

            Settings = await _settingsService.GetSettingsAsync(_userId);
            IsDirty = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (IsBusy || !IsDirty) return;

        try
        {
            IsBusy = true;

            await _settingsService.SaveSettingsAsync(_userId, Settings);
            IsDirty = false;

            // Show success message
            await Application.Current!.MainPage!.DisplayAlert("Settings", "Settings saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetToDefaultsAsync()
    {
        var result = await Application.Current!.MainPage!.DisplayAlert(
            "Reset Settings",
            "Are you sure you want to reset all settings to default values?",
            "Reset",
            "Cancel");

        if (!result) return;

        try
        {
            IsBusy = true;

            Settings = await _settingsService.GetDefaultSettingsAsync();
            IsDirty = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to reset settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Property change handlers to mark settings as dirty
    partial void OnSettingsChanged(QuizSettings value)
    {
        IsDirty = true;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        Settings.Mode = Settings.Mode == "Single-op" ? "Random Mix" : "Single-op";
        IsDirty = true;
        OnPropertyChanged(nameof(Settings));
    }

    [RelayCommand]
    private void ToggleTimer()
    {
        Settings.TimerEnabled = !Settings.TimerEnabled;
        IsDirty = true;
    }

    [RelayCommand]
    private void ToggleSound()
    {
        Settings.SoundEnabled = !Settings.SoundEnabled;
        IsDirty = true;
    }

    [RelayCommand]
    private void ToggleVibration()
    {
        Settings.VibrationEnabled = !Settings.VibrationEnabled;
        IsDirty = true;
    }

    [RelayCommand]
    private void ToggleBiggerText()
    {
        Settings.BiggerText = !Settings.BiggerText;
        IsDirty = true;
    }

    [RelayCommand]
    private void ToggleHighContrast()
    {
        Settings.HighContrast = !Settings.HighContrast;
        IsDirty = true;
    }

    [RelayCommand]
    private void UpdateQuestionsPerSession(object parameter)
    {
        if (parameter is int count)
        {
            Settings.QuestionsPerSession = count;
            IsDirty = true;
        }
    }

    [RelayCommand]
    private void UpdateTimerSeconds(object parameter)
    {
        if (parameter is int seconds)
        {
            Settings.TimerSeconds = seconds;
            IsDirty = true;
        }
    }
}
 