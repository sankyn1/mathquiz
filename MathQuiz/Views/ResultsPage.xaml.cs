// File: Views/ResultsPage.xaml.cs
using MathQuiz.ViewModels;

namespace MathQuiz.Views;

[QueryProperty(nameof(SessionId), "SessionId")]
[QueryProperty(nameof(Score), "Score")]
[QueryProperty(nameof(Accuracy), "Accuracy")]
[QueryProperty(nameof(MaxStreak), "MaxStreak")]
[QueryProperty(nameof(HighestLevel), "HighestLevel")]
[QueryProperty(nameof(TotalQuestions), "TotalQuestions")]
public partial class ResultsPage : ContentPage
{
    private readonly ResultsViewModel _viewModel;

    public int SessionId { get; set; }
    public int Score { get; set; }
    public double Accuracy { get; set; }
    public int MaxStreak { get; set; }
    public int HighestLevel { get; set; }
    public int TotalQuestions { get; set; }

    public ResultsPage(ResultsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize(SessionId, Score, Accuracy, MaxStreak, HighestLevel, TotalQuestions);
    }
}
