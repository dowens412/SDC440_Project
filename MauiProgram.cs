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

        builder.Services.AddSingleton<AppState>();

        // NEW: database
        builder.Services.AddSingleton<DatabaseService>();

        // Existing services (we will modify these in the next steps to use DatabaseService)
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<EventService>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<EventsAllPage>();

        var app = builder.Build();

        // Seed DB once on startup (Week 3 demo)
        var db = app.Services.GetRequiredService<DatabaseService>();
        _ = db.SeedIfEmptyAsync();

        return app;
    }
}
