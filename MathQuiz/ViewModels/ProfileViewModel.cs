// File: ViewModels/ProfileViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;
using System.Collections.ObjectModel;

namespace MathQuiz.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IPersistenceService _persistence;
    private readonly IRandomNameService _randomName;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private User? _selectedUser;

    [ObservableProperty]
    private string _newUserName = string.Empty;

    [ObservableProperty]
    private bool _isCreatingUser;

    [ObservableProperty]
    private bool _hasUsers;

    [ObservableProperty]
    private string _selectedUserStatsText = "Select a profile to view stats";

    public ProfileViewModel(IPersistenceService persistence, IRandomNameService randomName)
    {
        _persistence = persistence;
        _randomName = randomName;
        Title = "Profiles";
    }

    public override async void OnAppearing()
    {
        await LoadUsersAsync();
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        try
        {
            IsBusy = true;

            var users = await _persistence.GetUsersAsync();

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            HasUsers = Users.Any();

            // Auto-select first user if available
            if (!HasUsers && SelectedUser == null)
            {
                SelectedUser = Users.FirstOrDefault();
                if (SelectedUser != null)
                {
                    await LoadUserStatsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profiles: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectUserAsync(User user)
    {
        if (IsBusy) return;

        SelectedUser = user;
        await LoadUserStatsAsync();
    }

    [RelayCommand]
    private void StartCreateUser()
    {
        IsCreatingUser = true;
        NewUserName = string.Empty;
    }

    [RelayCommand]
    private void CancelCreateUser()
    {
        IsCreatingUser = false;
        NewUserName = string.Empty;
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(NewUserName))
            return;

        try
        {
            IsBusy = true;

            var userId = await _persistence.CreateUserAsync(NewUserName.Trim());
            var newUser = await _persistence.GetUserAsync(userId);

            if (newUser != null)
            {
                Users.Add(newUser);
                SelectedUser = newUser;
                await LoadUserStatsAsync();
            }

            IsCreatingUser = false;
            NewUserName = string.Empty;
            HasUsers = Users.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateRandomUserAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var randomName = _randomName.GenerateRandomName();
            var userId = await _persistence.CreateUserAsync(randomName);
            var newUser = await _persistence.GetUserAsync(userId);

            if (newUser != null)
            {
                Users.Add(newUser);
                SelectedUser = newUser;
                await LoadUserStatsAsync();
            }

            HasUsers = Users.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create random profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User user)
    {
        if (IsBusy || user == null) return;

        // Confirm deletion
        var result = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Profile",
            $"Are you sure you want to delete '{user.DisplayName}'? This will also delete all game history.",
            "Delete",
            "Cancel");

        if (!result) return;

        try
        {
            IsBusy = true;

            await _persistence.DeleteUserAsync(user.UserId);
            Users.Remove(user);

            if (SelectedUser?.UserId == user.UserId)
            {
                SelectedUser = Users.FirstOrDefault();
                if (SelectedUser != null)
                {
                    await LoadUserStatsAsync();
                }
                else
                {
                    SelectedUserStatsText = "No profiles available";
                }
            }

            HasUsers = Users.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RenameUserAsync(User user)
    {
        if (IsBusy || user == null) return;

        var newName = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Rename Profile",
            "Enter new name:",
            "OK",
            "Cancel",
            user.DisplayName);

        if (string.IsNullOrWhiteSpace(newName) || newName == user.DisplayName)
            return;

        try
        {
            IsBusy = true;

            user.DisplayName = newName.Trim();
            await _persistence.UpdateUserAsync(user);

            // Refresh the user in the collection
            var index = Users.ToList().FindIndex(u => u.UserId == user.UserId);
            if (index >= 0)
            {
                Users[index] = user;
            }

            if (SelectedUser?.UserId == user.UserId)
            {
                SelectedUser = user;
                await LoadUserStatsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to rename profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadUserStatsAsync()
    {
        if (SelectedUser == null)
        {
            SelectedUserStatsText = "No profile selected";
            return;
        }

        try
        {
            var sessions = await _persistence.GetSessionsAsync(SelectedUser.UserId);
            var completedSessions = sessions.Where(s => s.EndedAt != null).ToList();

            if (!completedSessions.Any())
            {
                SelectedUserStatsText = $"{SelectedUser.DisplayName}\n\nNo games played yet";
                return;
            }

            var totalGames = completedSessions.Count;
            var totalScore = completedSessions.Sum(s => s.Score);
            var averageScore = totalScore / totalGames;
            var averageAccuracy = completedSessions.Average(s => s.Accuracy);
            var bestScore = completedSessions.Max(s => s.Score);
            var maxStreak = completedSessions.Max(s => s.MaxStreak);
            var highestLevel = completedSessions.Max(s => s.HighestLevel);

            SelectedUserStatsText = $"{SelectedUser.DisplayName}\n\n" +
                                  $"Games Played: {totalGames}\n" +
                                  $"Best Score: {bestScore}\n" +
                                  $"Average Score: {averageScore:F0}\n" +
                                  $"Average Accuracy: {averageAccuracy:F1}%\n" +
                                  $"Best Streak: {maxStreak}\n" +
                                  $"Highest Level: {highestLevel}";
        }
        catch (Exception ex)
        {
            SelectedUserStatsText = $"Error loading stats: {ex.Message}";
        }
    }
}
