using MathQuiz.ViewModels;

namespace MathQuiz.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel viewModel)
        {
            viewModel.OnAppearing();
        }
    }
}
