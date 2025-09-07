// File: Views/LeaderboardPage.xaml.cs
using MathQuiz.ViewModels;

namespace MathQuiz.Views;

public partial class LeaderboardPage : ContentPage
{
    public LeaderboardPage(LeaderboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LeaderboardViewModel viewModel)
        {
            viewModel.OnAppearing();
        }
    }
}
