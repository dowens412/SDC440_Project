using Microsoft.Extensions.Logging;
using RSVPApp.Pages;
using RSVPApp.Services;
using RSVPApp.State;

namespace RSVPApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // App state
        builder.Services.AddSingleton<AppState>();

        // Database (local cache + RSVP duplicate protection)
        builder.Services.AddSingleton<DatabaseService>();

        // NEW: credential storage (Preferences instead of SecureStorage)
        builder.Services.AddSingleton<ICredentialStore, CredentialStore>();

        // Services
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<EventService>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<EventsAllPage>();
        builder.Services.AddTransient<AddEventPage>();
        builder.Services.AddTransient<EventDetailsPage>();

        var app = builder.Build();

        // Ensure DB tables exist (Week 4)
        var db = app.Services.GetRequiredService<DatabaseService>();
        _ = db.InitAsync();

        return app;
    }
}