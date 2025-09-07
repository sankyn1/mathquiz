// File: Services/IQuestionGenerator.cs
using MathQuiz.Models;

namespace MathQuiz.Services;

public interface IQuestionGenerator
{
    Task<Question> GenerateQuestionAsync(int sessionId, int level, CancellationToken cancellationToken = default);
    bool SupportsLevel(int level);
    string Mode { get; }
}

public class QuestionGenerationResult
{
    public string Expression { get; set; } = string.Empty;
    public int CorrectAnswer { get; set; }
    public int[] Options { get; set; } = new int[4];
    public int CorrectIndex { get; set; }
}
