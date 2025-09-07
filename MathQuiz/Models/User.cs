// File: Models/User.cs
using SQLite;

namespace MathQuiz.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int UserId { get; set; }

    [NotNull]
    public string DisplayName { get; set; } = string.Empty;

    [NotNull]
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("O");

    public User() { }

    public User(string displayName)
    {
        DisplayName = displayName;
        CreatedAt = DateTime.UtcNow.ToString("O");
    }
}
