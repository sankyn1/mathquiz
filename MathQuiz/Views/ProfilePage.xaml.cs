// File: Views/ProfilePage.xaml.cs
using MathQuiz.ViewModels;

namespace MathQuiz.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel viewModel)
        {
            viewModel.OnAppearing();
        }
    }
}
