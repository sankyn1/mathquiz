// File: ViewModels/ResultsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;
using MathQuiz.Views;

namespace MathQuiz.ViewModels;

public partial class ResultsViewModel : BaseViewModel
{
    private readonly IPersistenceService _persistence;

    [ObservableProperty]
    private string _scoreText = "0";

    [ObservableProperty]
    private string _accuracyText = "0%";

    [ObservableProperty]
    private string _streakText = "0";

    [ObservableProperty]
    private string _levelText = "1";

    [ObservableProperty]
    private string _totalQuestionsText = "0";

    [ObservableProperty]
    private string _resultMessage = "Great effort!";

    [ObservableProperty]
    private bool _showCelebration;

    [ObservableProperty]
    private string _performanceLevel = "Good";

    private int _userId;
    private int _score;
    private double _accuracy;
    private int _maxStreak;
    private int _highestLevel;
    private int _totalQuestions;

    public ResultsViewModel(IPersistenceService persistence)
    {
        _persistence = persistence;
        Title = "Results";
    }

    public void Initialize(int sessionId, int score, double accuracy, int maxStreak, int highestLevel, int totalQuestions)
    {
        _score = score;
        _accuracy = accuracy;
        _maxStreak = maxStreak;
        _highestLevel = highestLevel;
        _totalQuestions = totalQuestions;

        UpdateDisplayValues();
        DeterminePerformanceLevel();
        SetResultMessage();
    }

    [RelayCommand]
    private async Task PlayAgainAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var navigationParameter = new Dictionary<string, object>
            {
                ["UserId"] = _userId,
                ["Mode"] = "Random Mix" // Default to Random Mix
            };

            await Shell.Current.GoToAsync($"//{nameof(QuizPage)}", navigationParameter);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to start new game: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ViewLeaderboardAsync()
    {
        if (IsBusy) return;

        await Shell.Current.GoToAsync($"//{nameof(LeaderboardPage)}");
    }

    [RelayCommand]
    private async Task GoHomeAsync()
    {
        if (IsBusy) return;

        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    [RelayCommand]
    private async Task ShareResultsAsync()
    {
        // Simple sharing implementation
        try
        {
            var shareText = $"I just scored {_score} points with {_accuracy:F1}% accuracy in Math Quiz! 🎉";

            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
            {
                Text = shareText,
                Title = "Math Quiz Results"
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Sharing failed: {ex.Message}";
        }
    }

    private void UpdateDisplayValues()
    {
        ScoreText = _score.ToString();
        AccuracyText = $"{_accuracy:F1}%";
        StreakText = $"{_maxStreak}";
        LevelText = $"{_highestLevel}";
        TotalQuestionsText = $"{_totalQuestions}";
    }

    private void DeterminePerformanceLevel()
    {
        PerformanceLevel = _accuracy switch
        {
            >= 90 => "Excellent",
            >= 75 => "Great",
            >= 60 => "Good",
            >= 40 => "Not Bad",
            _ => "Keep Trying"
        };

        // Show celebration for excellent performance
        ShowCelebration = _accuracy >= 80 || _maxStreak >= 5;
    }

    private void SetResultMessage()
    {
        ResultMessage = PerformanceLevel switch
        {
            "Excellent" => "Outstanding work! You're a math superstar! 🌟",
            "Great" => "Fantastic job! Keep up the great work! 🎉",
            "Good" => "Nice job! You're getting better! 👍",
            "Not Bad" => "Good effort! Practice makes perfect! 💪",
            _ => "Keep practicing! You'll improve with time! 😊"
        };
    }
}
