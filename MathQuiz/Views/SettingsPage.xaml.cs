// File: Views/SettingsPage.xaml.cs
using MathQuiz.ViewModels;

namespace MathQuiz.Views;

[QueryProperty(nameof(UserId), "UserId")]
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public int UserId { get; set; } = 1;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(UserId);
    }
}
