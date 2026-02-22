using RSVPApp.Models;
using RSVPApp.Services;
using RSVPApp.State;

namespace RSVPApp.Pages;

public partial class AddEventPage : ContentPage
{
    private readonly EventService _events;
    private readonly AppState _state;

    public AddEventPage(EventService events, AppState state)
    {
        InitializeComponent();
        _events = events;
        _state = state;

        // Defaults
        EventDatePicker.Date = DateTime.Today.AddDays(7);
        DeadlineDatePicker.Date = DateTime.Today.AddDays(5);
    }

    private static TimeSpan ToTimeSpan(object timeValue)
    {
        // Handles TimeSpan, TimeOnly (net6+/net7+/net8+), and falls back safely
        if (timeValue is TimeSpan ts) return ts;

#if NET6_0_OR_GREATER
        if (timeValue is TimeOnly to) return to.ToTimeSpan();
#endif

        // If it’s something unexpected, default to midnight
        return TimeSpan.Zero;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "";

        try
        {
            if (_state.CurrentUser == null)
                throw new Exception("You must be logged in.");

            var hostName = HostNameEntry.Text?.Trim() ?? "";
            var eventName = EventNameEntry.Text?.Trim() ?? "";
            var address = AddressEntry.Text?.Trim() ?? "";

            if (!int.TryParse(MaxEntry.Text?.Trim(), out var maxAllowed) || maxAllowed <= 0)
                throw new Exception("Max allowed attendees must be a number greater than 0.");

            if (string.IsNullOrWhiteSpace(hostName) ||
                string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(address))
                throw new Exception("Fill in all fields.");

            // DatePicker.Date is nullable in your project (DateTime?)
            var eventDate = EventDatePicker.Date ?? DateTime.Today.AddDays(7);
            var deadlineDate = DeadlineDatePicker.Date ?? DateTime.Today.AddDays(5);

            // Convert picker time to TimeSpan safely
            var eventTime = ToTimeSpan(EventTimePicker.Time);
            var deadlineTime = ToTimeSpan(DeadlineTimePicker.Time);

            var eventLocal = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 0, 0, 0, DateTimeKind.Local)
                .Add(eventTime);

            var deadlineLocal = new DateTime(deadlineDate.Year, deadlineDate.Month, deadlineDate.Day, 0, 0, 0, DateTimeKind.Local)
                .Add(deadlineTime);

            if (deadlineLocal > eventLocal)
                throw new Exception("Deadline must be before the event date/time.");

            var req = new ApiEventCreateRequest(
                HostUserId: _state.CurrentUser.Id,
                HostName: hostName,
                EventName: eventName,
                EventAddress: address,
                MaxAllowedAttendees: maxAllowed,
                EventDateTimeUtc: eventLocal.ToUniversalTime(),
                RsvpDeadlineUtc: deadlineLocal.ToUniversalTime()
            );

            await _events.AddEventAsync(req);

            await DisplayAlert("Saved", "Event created.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}