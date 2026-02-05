using RSVPApp.Pages;

namespace RSVPApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Routes (ShellContent routes exist, but registering is fine and helps)
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(EventsAllPage), typeof(EventsAllPage));
    }
}
