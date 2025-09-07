// File: Views/QuizPage.xaml.cs
using MathQuiz.ViewModels;

namespace MathQuiz.Views;

[QueryProperty(nameof(UserId), "UserId")]
[QueryProperty(nameof(Mode), "Mode")]
public partial class QuizPage : ContentPage
{
    private readonly QuizViewModel _viewModel;

    public int UserId { get; set; }
    public string Mode { get; set; } = "Random Mix";

    public QuizPage(QuizViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (UserId > 0)
        {
            await _viewModel.InitializeAsync(UserId, Mode);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}
