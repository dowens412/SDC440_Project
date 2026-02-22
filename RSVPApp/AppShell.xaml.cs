using RSVPApp.Pages;

namespace RSVPApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Routes
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("addEvent", typeof(AddEventPage));
        Routing.RegisterRoute("eventDetails", typeof(EventDetailsPage));

        // Start route (login)
        GoToAsync("//login");
    }
}