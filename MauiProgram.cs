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

        // App state + services
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<AuthService>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<EventsAllPage>();

        return builder.Build();
    }
}
