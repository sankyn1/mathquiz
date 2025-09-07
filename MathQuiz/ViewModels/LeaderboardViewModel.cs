// File: ViewModels/LeaderboardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;
using System.Collections.ObjectModel;

namespace MathQuiz.ViewModels;

public partial class LeaderboardViewModel : BaseViewModel
{
    private readonly IPersistenceService _persistence;

    [ObservableProperty]
    private ObservableCollection<SessionDisplayModel> _topSessions = new();

    [ObservableProperty]
    private ObservableCollection<SessionDisplayModel> _recentSessions = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _hasData = false;

    [ObservableProperty]
    private string _emptyMessage = "No games played yet. Start playing to see your scores here!";

    private int? _currentUserId;

    public LeaderboardViewModel(IPersistenceService persistence)
    {
        _persistence = persistence;
        Title = "Leaderboard";
    }

    public override async void OnAppearing()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterByCurrentUserAsync()
    {
        // Toggle between all users and current user
        if (_currentUserId.HasValue)
        {
            _currentUserId = null;
            Title = "All Players";
        }
        else
        {
            // Get current user (first user for simplicity)
            var users = await _persistence.GetUsersAsync();
            _currentUserId = users.FirstOrDefault()?.UserId;
            Title = "My Scores";
        }

        await LoadDataAsync();
    }

    [RelayCommand]
    private void TabSelectionChanged(object parameter)
    {
        if (parameter is int index)
        {
            SelectedTabIndex = index;
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            // Load top sessions
            var topSessions = await _persistence.GetTopSessionsAsync(10, _currentUserId);
            TopSessions.Clear();

            foreach (var session in topSessions)
            {
                var user = await _persistence.GetUserAsync(session.UserId);
                TopSessions.Add(new SessionDisplayModel(session, user?.DisplayName ?? "Unknown"));
            }

            // Load recent sessions
            var recentSessions = await _persistence.GetSessionsAsync(_currentUserId, 20);
            RecentSessions.Clear();

            foreach (var session in recentSessions.Where(s => s.EndedAt != null))
            {
                var user = await _persistence.GetUserAsync(session.UserId);
                RecentSessions.Add(new SessionDisplayModel(session, user?.DisplayName ?? "Unknown"));
            }

            HasData = TopSessions.Any() || RecentSessions.Any();

            if (!HasData)
            {
                EmptyMessage = _currentUserId.HasValue
                    ? "You haven't played any games yet. Start playing to see your scores!"
                    : "No games played yet. Be the first to play!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load leaderboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class SessionDisplayModel
{
    public SessionDisplayModel(Session session, string playerName)
    {
        Session = session;
        PlayerName = playerName;
        ScoreText = $"{session.Score} pts";
        AccuracyText = $"{session.Accuracy:F1}%";
        DateText = DateTime.Parse(session.StartedAt).ToString("MMM dd, yyyy");
        TimeText = DateTime.Parse(session.StartedAt).ToString("HH:mm");
        ModeText = session.Mode;
        StreakText = $"Max: {session.MaxStreak}";
        LevelText = $"Lvl {session.HighestLevel}";

        // Performance indicator
        PerformanceColor = session.Accuracy switch
        {
            >= 90 => "#4CAF50", // Green
            >= 75 => "#FF9800", // Orange
            >= 60 => "#2196F3", // Blue
            _ => "#757575"      // Gray
        };

        RankIcon = session.Accuracy >= 90 ? "🏆" :
                   session.Accuracy >= 75 ? "🥈" :
                   session.Accuracy >= 60 ? "🥉" : "📊";
    }

    public Session Session { get; }
    public string PlayerName { get; }
    public string ScoreText { get; }
    public string AccuracyText { get; }
    public string DateText { get; }
    public string TimeText { get; }
    public string ModeText { get; }
    public string StreakText { get; }
    public string LevelText { get; }
    public string PerformanceColor { get; }
    public string RankIcon { get; }
}
