using RSVPApp.Services;
using RSVPApp.State;

namespace RSVPApp.Pages;

public partial class EventsAllPage : ContentPage
{
    private readonly AppState _state;
    private readonly EventService _events;

    public EventsAllPage(AppState state, EventService events)
    {
        InitializeComponent();
        _state = state;
        _events = events;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        HeaderLabel.Text = _state.IsLoggedIn
            ? $"All Events (Logged In: {_state.UserEmail})"
            : "All Events (Guest)";

        EventsList.ItemsSource = _events.GetAllEvents();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _state.Logout();
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

    private async void OnEventSelected(object sender, SelectionChangedEventArgs e)
    {
        // Clear selection so you can tap again
        var selected = e.CurrentSelection.FirstOrDefault();
        EventsList.SelectedItem = null;

        if (selected == null) return;

        // Details page comes next step.
        await DisplayAlert("Next Step", "Event Details page is coming next.", "OK");
    }
}
