// File: ViewModels/QuizViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathQuiz.Models;
using MathQuiz.Services;
using MathQuiz.Views;
using System.Diagnostics;

namespace MathQuiz.ViewModels;

public partial class QuizViewModel : BaseViewModel
{
    private readonly IPersistenceService _persistence;
    private readonly IScoringService _scoring;
    private readonly ISettingsService _settingsService;
    private readonly SingleOpGenerator _singleOpGenerator;
    private readonly MixedExpressionGenerator _mixedGenerator;

    [ObservableProperty]
    private GameState _gameState = new();

    [ObservableProperty]
    private Question? _currentQuestion;

    [ObservableProperty]
    private string _questionExpression = "";

    [ObservableProperty]
    private List<string> _answerOptions = new();

    [ObservableProperty]
    private bool _buttonsEnabled = true;

    [ObservableProperty]
    private string _feedbackMessage = "";

    [ObservableProperty]
    private bool _showFeedback;

    [ObservableProperty]
    private string _scoreText = "Score: 0";

    [ObservableProperty]
    private string _streakText = "Streak: 0";

    [ObservableProperty]
    private string _levelText = "Level: 1";

    [ObservableProperty]
    private string _questionCountText = "1 / 10";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private int _timeRemaining;

    [ObservableProperty]
    private bool _timerVisible;

    private QuizSettings? _settings;
    private Session? _currentSession;
    private Timer? _questionTimer;
    private Timer? _sessionTimer;
    private Stopwatch _answerStopwatch = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public QuizViewModel(
        IPersistenceService persistence,
        IScoringService scoring,
        ISettingsService settingsService,
        SingleOpGenerator singleOpGenerator,
        MixedExpressionGenerator mixedGenerator)
    {
        _persistence = persistence;
        _scoring = scoring;
        _settingsService = settingsService;
        _singleOpGenerator = singleOpGenerator;
        _mixedGenerator = mixedGenerator;

        Title = "Quiz";
    }

    public async Task InitializeAsync(int userId, string mode)
    {
        try
        {
            IsBusy = true;
            _cancellationTokenSource = new CancellationTokenSource();

            _settings = await _settingsService.GetSettingsAsync(userId);
            _settings.Mode = mode; // Override with passed mode

            // Initialize game state
            GameState = new GameState
            {
                CurrentLevel = 1,
                CurrentStreak = 0,
                Score = 0,
                SessionStart = DateTime.UtcNow
            };

            // Create session
            _currentSession = new Session(userId, mode, _settings.QuestionsPerSession);
            await _persistence.CreateSessionAsync(_currentSession);

            // Update HUD
            UpdateHUD();

            // Setup timer if enabled
            if (_settings.TimerEnabled)
            {
                TimeRemaining = _settings.TimerSeconds;
                TimerVisible = true;
                StartSessionTimer();
            }

            // Generate and show first question
            await NextQuestionAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to initialize quiz: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AnswerSelectedAsync(object parameter)
    {
        if (!ButtonsEnabled || CurrentQuestion == null || parameter is not string indexStr)
            return;

        if (!int.TryParse(indexStr, out var selectedIndex) || selectedIndex < 0 || selectedIndex > 3)
            return;

        await ProcessAnswerAsync(selectedIndex);
    }

    private async Task ProcessAnswerAsync(int selectedIndex)
    {
        try
        {
            ButtonsEnabled = false;
            _answerStopwatch.Stop();

            var isCorrect = selectedIndex == CurrentQuestion!.CorrectIndex;
            var answerTimeMs = (int)_answerStopwatch.ElapsedMilliseconds;

            // Save attempt
            var attempt = new Attempt(CurrentQuestion.QuestionId, selectedIndex, isCorrect, answerTimeMs);
            await _persistence.SaveAttemptAsync(attempt);
            GameState.Attempts.Add(attempt);

            // Update game state
            if (isCorrect)
            {
                GameState.CorrectAnswers++;
                GameState.CurrentStreak++;
                GameState.MaxStreak = Math.Max(GameState.MaxStreak, GameState.CurrentStreak);
                GameState.ConsecutiveWrong = 0;

                // Update score
                var newScore = _scoring.CalculateScore(
                    GameState.Score,
                    GameState.CurrentStreak,
                    isCorrect,
                    answerTimeMs,
                    _settings?.TimerEnabled ?? false);
                GameState.Score = newScore;

                // Show positive feedback
                FeedbackMessage = GetPositiveFeedback();
                await ShowCorrectFeedbackAsync();

                // Check for level up (every 3 correct in a row)
                if (GameState.CurrentStreak % 3 == 0)
                {
                    GameState.CurrentLevel++;
                    GameState.UpdateLevel();
                }
            }
            else
            {
                GameState.WrongAnswers++;
                GameState.CurrentStreak = 0;
                GameState.ConsecutiveWrong++;

                // Show negative feedback
                FeedbackMessage = GetNegativeFeedback(CurrentQuestion.CorrectAnswer);
                await ShowIncorrectFeedbackAsync();

                // Check for level down (after 2 wrong)
                if (GameState.ConsecutiveWrong >= 2 && GameState.CurrentLevel > 1)
                {
                    GameState.CurrentLevel--;
                    GameState.ConsecutiveWrong = 0;
                }
            }

            UpdateHUD();

            // Auto-advance to next question after feedback
            await Task.Delay(1500, _cancellationTokenSource?.Token ?? CancellationToken.None);

            if (!_cancellationTokenSource?.Token.IsCancellationRequested ?? false)
            {
                await NextQuestionAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing answer: {ex.Message}";
            ButtonsEnabled = true;
        }
    }

    private async Task NextQuestionAsync()
    {
        try
        {
            ShowFeedback = false;

            // Check if quiz is complete
            if (GameState.CurrentQuestionIndex >= _settings!.QuestionsPerSession)
            {
                await EndQuizAsync();
                return;
            }

            // Generate next question
            var generator = _settings.Mode == "Single-op" ?
                (IQuestionGenerator)_singleOpGenerator :
                _mixedGenerator;

            CurrentQuestion = await generator.GenerateQuestionAsync(
                _currentSession!.SessionId,
                GameState.CurrentLevel,
                _cancellationTokenSource?.Token ?? CancellationToken.None);

            // Save question
            await _persistence.SaveQuestionAsync(CurrentQuestion);
            GameState.Questions.Add(CurrentQuestion);

            // Update UI
            QuestionExpression = CurrentQuestion.Expression;
            AnswerOptions = CurrentQuestion.GetOptions().Select(o => o.ToString()).ToList();

            GameState.CurrentQuestionIndex++;
            UpdateProgressAndCount();

            // Enable buttons and start timing
            ButtonsEnabled = true;
            _answerStopwatch.Restart();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate question: {ex.Message}";
        }
    }

    private async Task EndQuizAsync()
    {
        try
        {
            StopTimers();

            // Update and save session
            if (_currentSession != null)
            {
                _currentSession.EndSession(
                    GameState.Score,
                    GameState.Accuracy,
                    GameState.MaxStreak,
                    GameState.HighestLevel);

                await _persistence.UpdateSessionAsync(_currentSession);
            }

            // Navigate to results
            var navigationParameter = new Dictionary<string, object>
            {
                ["SessionId"] = _currentSession?.SessionId ?? 0,
                ["Score"] = GameState.Score,
                ["Accuracy"] = GameState.Accuracy,
                ["MaxStreak"] = GameState.MaxStreak,
                ["HighestLevel"] = GameState.HighestLevel,
                ["TotalQuestions"] = _settings!.QuestionsPerSession
            };

            await Shell.Current.GoToAsync(nameof(ResultsPage), navigationParameter);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to end quiz: {ex.Message}";
        }
    }

    private void UpdateHUD()
    {
        ScoreText = $"Score: {GameState.Score}";
        StreakText = GameState.CurrentStreak > 0 ? $"🔥 {GameState.CurrentStreak}" : "Streak: 0";
        LevelText = $"Level: {GameState.CurrentLevel}";
    }

    private void UpdateProgressAndCount()
    {
        var total = _settings?.QuestionsPerSession ?? 10;
        ProgressValue = (double)GameState.CurrentQuestionIndex / total;
        QuestionCountText = $"{GameState.CurrentQuestionIndex} / {total}";
    }

    private void StartSessionTimer()
    {
        if (!_settings?.TimerEnabled ?? true) return;

        _sessionTimer = new Timer(OnSessionTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void OnSessionTimerTick(object? state)
    {
        TimeRemaining--;

        if (TimeRemaining <= 0)
        {
            MainThread.BeginInvokeOnMainThread(async () => await EndQuizAsync());
        }
    }

    private void StopTimers()
    {
        _questionTimer?.Dispose();
        _sessionTimer?.Dispose();
        _answerStopwatch.Stop();
    }

    private string GetPositiveFeedback()
    {
        var messages = new[] { "Correct! 🎉", "Great job! ✨", "Excellent! 🌟", "Perfect! 👏", "Amazing! 🚀" };
        return messages[Random.Shared.Next(messages.Length)];
    }

    private string GetNegativeFeedback(int correctAnswer)
    {
        var messages = new[]
        {
            $"Not quite! The answer was {correctAnswer} 😊",
            $"Close! It's {correctAnswer} 💪",
            $"Try again next time! Answer: {correctAnswer} 🤗"
        };
        return messages[Random.Shared.Next(messages.Length)];
    }

    private async Task ShowCorrectFeedbackAsync()
    {
        ShowFeedback = true;
        
    }

    private async Task ShowIncorrectFeedbackAsync()
    {
        ShowFeedback = true;
        // Trigger different haptic feedback
       
    }

    public override void OnDisappearing()
    {
        StopTimers();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
