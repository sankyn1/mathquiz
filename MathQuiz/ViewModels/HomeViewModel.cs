// File: ViewModels/HomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;
using MathQuiz.Views;


namespace MathQuiz.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IPersistenceService _persistence;
    private readonly ISettingsService _settings;
    private readonly IRandomNameService _randomName;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private string _welcomeMessage = "Welcome to Math Quiz!";

    [ObservableProperty]
    private string _quickStartText = "Quick Start";

    [ObservableProperty]
    private bool _hasUsers;

    public HomeViewModel(IPersistenceService persistence, ISettingsService settings, IRandomNameService randomName)
    {
        _persistence = persistence;
        _settings = settings;
        _randomName = randomName;
        Title = "Math Quiz";
    }

    public override async void OnAppearing()
    {
        await LoadCurrentUserAsync();
    }

    [RelayCommand]
    private async Task QuickStartAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Get or create current user
            if (CurrentUser == null)
            {
                await CreateQuickUserAsync();
            }

            // Navigate to quiz
            var navigationParameter = new Dictionary<string, object>
            {
                ["UserId"] = CurrentUser!.UserId,
                ["Mode"] = "Random Mix"
            };

            await Shell.Current.GoToAsync(nameof(QuizPage), navigationParameter);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to start quiz: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectUserAsync()
    {
        if (IsBusy) return;

        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    [RelayCommand]
    private async Task ViewLeaderboardAsync()
    {
        if (IsBusy) return;

        await Shell.Current.GoToAsync(nameof(LeaderboardPage));
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (IsBusy) return;

        var navigationParameter = new Dictionary<string, object>
        {
            ["UserId"] = CurrentUser?.UserId ?? 1
        };

        await Shell.Current.GoToAsync(nameof(SettingsPage), navigationParameter);
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            var users = await _persistence.GetUsersAsync();
            HasUsers = users.Any();

            // Use first user as current, or null if no users
            CurrentUser = users.FirstOrDefault();

            if (CurrentUser != null)
            {
                WelcomeMessage = $"Welcome back, {CurrentUser.DisplayName}!";
            }
            else
            {
                WelcomeMessage = "Welcome to Math Quiz!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load user: {ex.Message}";
        }
    }

    private async Task CreateQuickUserAsync()
    {
        var randomName = _randomName.GenerateRandomName();
        var userId = await _persistence.CreateUserAsync(randomName);

        CurrentUser = await _persistence.GetUserAsync(userId);
        WelcomeMessage = $"Welcome, {CurrentUser!.DisplayName}!";
    }
}
