using RSVPApp.Models;
using RSVPApp.Services;

namespace RSVPApp.Pages;

[QueryProperty(nameof(EventId), "eventId")]
public partial class EventDetailsPage : ContentPage
{
    private readonly EventService _events;

    public string EventId { get; set; } = "";

    private ApiEventDetails? _details;

    public EventDetailsPage(EventService events)
    {
        InitializeComponent();
        _events = events;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        StatusLabel.Text = "";

        try
        {
            if (string.IsNullOrWhiteSpace(EventId))
                throw new Exception("Missing event id.");

            _details = await _events.GetEventDetailsAsync(EventId);

            TitleLabel.Text = _details.EventName;
            HostLabel.Text = $"Host: {_details.HostName}";
            AddressLabel.Text = $"Address: {_details.EventAddress}";
            EventDateLabel.Text = $"Event: {_details.EventDateTimeUtc.ToLocalTime():f}";
            DeadlineLabel.Text = $"RSVP Deadline: {_details.RsvpDeadlineUtc.ToLocalTime():f}";
            CountLabel.Text = $"Attending: {_details.CurrentAttendeeCount} / {_details.MaxAllowedAttendees}";

            AttendeesList.ItemsSource = _details.AttendeeNames;

            // Basic UI hint
            if (DateTime.UtcNow > _details.RsvpDeadlineUtc)
                RsvpButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }

    private async void OnRsvpClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "";

        try
        {
            if (_details == null) return;

            await _events.RSVPAsync(_details);

            await DisplayAlert("RSVP Confirmed", "You are signed up for this event.", "OK");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}