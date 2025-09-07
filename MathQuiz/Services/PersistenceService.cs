// File: Services/PersistenceService.cs
using MathQuiz.Data;
using MathQuiz.Models;

namespace MathQuiz.Services;

public class PersistenceService : IPersistenceService
{
    private readonly LocalDatabase _database;

    public PersistenceService(LocalDatabase database)
    {
        _database = database;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _database.GetUsersAsync();
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _database.GetUserAsync(userId);
    }

    public async Task<int> CreateUserAsync(string displayName)
    {
        var user = new User(displayName);
        await _database.InsertUserAsync(user);

        // Create default settings for new user
        var settings = new Settings(user.UserId);
        await _database.UpsertSettingsAsync(settings);

        return user.UserId;
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        return await _database.UpdateUserAsync(user);
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _database.GetUserAsync(userId);
        if (user != null)
            await _database.DeleteUserAsync(user);
    }

    public async Task<List<Session>> GetSessionsAsync(int? userId = null, int limit = 50)
    {
        var sessions = await _database.GetSessionsAsync(userId);
        return sessions.Take(limit).ToList();
    }

    public async Task<List<Session>> GetTopSessionsAsync(int limit = 10, int? userId = null)
    {
        return await _database.GetTopSessionsAsync(limit, userId);
    }

    public async Task<Session?> GetSessionAsync(int sessionId)
    {
        return await _database.GetSessionAsync(sessionId);
    }

    public async Task<int> CreateSessionAsync(Session session)
    {
        await _database.InsertSessionAsync(session);
        return session.SessionId;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        await _database.UpdateSessionAsync(session);
    }

    public async Task<Settings?> GetSettingsAsync(int userId)
    {
        return await _database.GetSettingsAsync(userId);
    }

    public async Task SaveSettingsAsync(Settings settings)
    {
        await _database.UpsertSettingsAsync(settings);
    }

    public async Task<int> SaveQuestionAsync(Question question)
    {
        await _database.InsertQuestionAsync(question);
        return question.QuestionId;
    }

    public async Task<int> SaveAttemptAsync(Attempt attempt)
    {
        await _database.InsertAttemptAsync(attempt);
        return attempt.AttemptId;
    }
}
