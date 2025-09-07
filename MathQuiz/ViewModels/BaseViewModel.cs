// File: ViewModels/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace MathQuiz.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public virtual void OnAppearing() { }
    public virtual void OnDisappearing() { }
}
