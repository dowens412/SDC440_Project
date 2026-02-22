namespace RSVPApp.Models;

public class RsvpEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HostName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Address { get; set; } = "";
    public int MaxAttendees { get; set; }
    public int CurrentAttendees { get; set; }
    public DateTime EventDateTime { get; set; }
    public DateTime RsvpDeadline { get; set; }

    public List<string> AttendeeNames { get; set; } = new();
}
