using RSVPApp.Models;
using RSVPApp.Services;

namespace RSVPApp.Pages;

public partial class EventsAllPage : ContentPage
{
    private readonly EventService _events;

    private List<ApiEventSummary> _current = new();

    public EventsAllPage(EventService events)
    {
        InitializeComponent();
        _events = events;

        FilterPicker.SelectedIndex = 0; // All
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        StatusLabel.Text = "";

        try
        {
            var filter = FilterPicker.SelectedItem?.ToString() ?? "All";

            if (filter == "Hosting")
                _current = await _events.GetHostingEventsAsync();
            else if (filter == "Attending")
                _current = await _events.GetAttendingEventsAsync();
            else
                _current = await _events.GetAllEventsAsync();

            EventsList.ItemsSource = _current;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        await ReloadAsync();
    }

    private async void OnAddEventClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addEvent");
    }

    private async void OnEventSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection?.FirstOrDefault() as ApiEventSummary;
        if (selected == null) return;

        // clear selection
        EventsList.SelectedItem = null;

        await Shell.Current.GoToAsync($"eventDetails?eventId={Uri.EscapeDataString(selected.Id)}");
    }
}