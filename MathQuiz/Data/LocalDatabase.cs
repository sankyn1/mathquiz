// File: Data/LocalDatabase.cs
using SQLite;
using MathQuiz.Models;

namespace MathQuiz.Data;

public class LocalDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly string _databasePath;

    public LocalDatabase()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "MathQuiz.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Session>();
        await _database.CreateTableAsync<Question>();
        await _database.CreateTableAsync<Attempt>();
        await _database.CreateTableAsync<Settings>();
    }

    public async Task InitializeDatabaseAsync()
    {
        await InitAsync();
        await SeedDataAsync();
    }

    private async Task SeedDataAsync()
    {
        var userCount = await _database!.Table<User>().CountAsync();
        if (userCount == 0)
        {
            // Create default user
            var defaultUser = new User("Quick Player");
            await _database.InsertAsync(defaultUser);

            // Create default settings
            var settings = new Settings(1); // UserId 1
            await _database.InsertAsync(settings);

            // Create sample session for demo
            var session = new Session(1, "Single-op", 5);
            session.EndSession(120, 80.0, 3, 2);
            await _database.InsertAsync(session);
        }
    }

    // Users
    public async Task<List<User>> GetUsersAsync()
    {
        await InitAsync();
        return await _database!.Table<User>().ToListAsync();
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        await InitAsync();
        return await _database!.Table<User>().Where(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserAsync(User user)
    {
        await InitAsync();
        return await _database!.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitAsync();
        return await _database!.UpdateAsync(user);
    }

    public async Task<int> DeleteUserAsync(User user)
    {
        await InitAsync();
        return await _database!.DeleteAsync(user);
    }

    // Sessions
    public async Task<List<Session>> GetSessionsAsync(int? userId = null)
    {
        await InitAsync();
        var query = _database!.Table<Session>();
        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);
        return await query.OrderByDescending(s => s.StartedAt).ToListAsync();
    }

    public async Task<Session?> GetSessionAsync(int sessionId)
    {
        await InitAsync();
        return await _database!.Table<Session>().Where(s => s.SessionId == sessionId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertSessionAsync(Session session)
    {
        await InitAsync();
        return await _database!.InsertAsync(session);
    }

    public async Task<int> UpdateSessionAsync(Session session)
    {
        await InitAsync();
        return await _database!.UpdateAsync(session);
    }

    public async Task<List<Session>> GetTopSessionsAsync(int limit = 10, int? userId = null)
    {
        await InitAsync();
        var query = _database!.Table<Session>();
        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);
        return await query.OrderByDescending(s => s.Score).Take(limit).ToListAsync();
    }

    // Questions
    public async Task<int> InsertQuestionAsync(Question question)
    {
        await InitAsync();
        return await _database!.InsertAsync(question);
    }

    public async Task<List<Question>> GetSessionQuestionsAsync(int sessionId)
    {
        await InitAsync();
        return await _database!.Table<Question>().Where(q => q.SessionId == sessionId).ToListAsync();
    }

    // Attempts
    public async Task<int> InsertAttemptAsync(Attempt attempt)
    {
        await InitAsync();
        return await _database!.InsertAsync(attempt);
    }

    public async Task<List<Attempt>> GetQuestionAttemptsAsync(int questionId)
    {
        await InitAsync();
        return await _database!.Table<Attempt>().Where(a => a.QuestionId == questionId).ToListAsync();
    }

    // Settings
    public async Task<Settings?> GetSettingsAsync(int userId)
    {
        await InitAsync();
        return await _database!.Table<Settings>().Where(s => s.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<int> UpsertSettingsAsync(Settings settings)
    {
        await InitAsync();
        var existing = await GetSettingsAsync(settings.UserId);
        if (existing == null)
            return await _database!.InsertAsync(settings);
        else
            return await _database!.UpdateAsync(settings);
    }
}
