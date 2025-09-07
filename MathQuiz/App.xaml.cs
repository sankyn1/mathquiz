// File: App.xaml.cs
using MathQuiz.Data;

namespace MathQuiz;

public partial class App : Application
{
    public App(LocalDatabase database)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Initialize database on app start
        Task.Run(async () => await database.InitializeDatabaseAsync());
    }
}
