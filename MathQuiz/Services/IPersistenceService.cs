// File: Services/IPersistenceService.cs
using MathQuiz.Models;

namespace MathQuiz.Services;

public interface IPersistenceService
{
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserAsync(int userId);
    Task<int> CreateUserAsync(string displayName);
    Task<int> UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);

    Task<List<Session>> GetSessionsAsync(int? userId = null, int limit = 50);
    Task<List<Session>> GetTopSessionsAsync(int limit = 10, int? userId = null);
    Task<Session?> GetSessionAsync(int sessionId);
    Task<int> CreateSessionAsync(Session session);
    Task UpdateSessionAsync(Session session);

    Task<Settings?> GetSettingsAsync(int userId);
    Task SaveSettingsAsync(Settings settings);

    Task<int> SaveQuestionAsync(Question question);
    Task<int> SaveAttemptAsync(Attempt attempt);
}
