// File: MauiProgram.cs
using CommunityToolkit.Maui;
using MathQuiz.Data;
using MathQuiz.Services;
using MathQuiz.ViewModels;
using MathQuiz.Views;
using Microsoft.Extensions.Logging;

namespace MathQuiz;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        //builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddLogging(logging => logging.AddDebug());
#endif

        // Database
        builder.Services.AddSingleton<LocalDatabase>();

        // Services
        builder.Services.AddSingleton<IPersistenceService, PersistenceService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IScoringService, ScoringService>();
        builder.Services.AddSingleton<IRandomNameService, RandomNameService>();
        builder.Services.AddTransient<SingleOpGenerator>();
        builder.Services.AddTransient<MixedExpressionGenerator>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<QuizViewModel>();
        builder.Services.AddTransient<ResultsViewModel>();
        builder.Services.AddTransient<LeaderboardViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<QuizPage>();
        builder.Services.AddTransient<ResultsPage>();
        builder.Services.AddTransient<LeaderboardPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
