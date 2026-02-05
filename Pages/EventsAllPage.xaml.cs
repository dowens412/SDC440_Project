using RSVPApp.State;

namespace RSVPApp.Pages;

public partial class EventsAllPage : ContentPage
{
    private readonly AppState _state;

    public EventsAllPage(AppState state)
    {
        InitializeComponent();
        _state = state;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_state.IsLoggedIn)
            HeaderLabel.Text = $"All Events (Logged In: {_state.UserEmail})";
        else
            HeaderLabel.Text = "All Events (Guest)";
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _state.Logout();
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}
