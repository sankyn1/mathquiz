// File: Services/RandomNameService.cs
namespace MathQuiz.Services;

public class RandomNameService : IRandomNameService
{
    private readonly Random _random = new();
    private readonly string[] _adjectives = {
        "Smart", "Quick", "Clever", "Bright", "Sharp", "Fast", "Super", "Amazing",
        "Cool", "Awesome", "Fun", "Happy", "Lucky", "Star", "Speedy", "Wizard"
    };

    private readonly string[] _nouns = {
        "Player", "Solver", "Thinker", "Champion", "Master", "Genius", "Hero",
        "Student", "Learner", "Explorer", "Discoverer", "Adventurer", "Builder"
    };

    public string GenerateRandomName()
    {
        var adjective = _adjectives[_random.Next(_adjectives.Length)];
        var noun = _nouns[_random.Next(_nouns.Length)];
        var number = _random.Next(1000, 9999);

        return $"{adjective} {noun} {number}";
    }
}
