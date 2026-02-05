using RSVPApp.Models;

namespace RSVPApp.Services;

public class EventService
{
    private readonly List<RsvpEvent> _events = new()
    {
        new RsvpEvent
        {
            Id = "E1",
            HostName = "Test User",
            EventName = "Garage Gym Meetup",
            Address = "123 Main St, Richmond, VA",
            MaxAttendees = 15,
            CurrentAttendees = 6,
            EventDateTime = DateTime.Now.AddDays(7).Date.AddHours(18),
            RsvpDeadline = DateTime.Now.AddDays(5).Date.AddHours(23).AddMinutes(59),
            AttendeeNames = new List<string> { "Test User", "Chris", "Jordan", "Sam", "Lee", "Taylor" }
        },
        new RsvpEvent
        {
            Id = "E2",
            HostName = "Morgan",
            EventName = "Nutrition Q&A",
            Address = "45 Fitness Ave, Charlotte, NC",
            MaxAttendees = 30,
            CurrentAttendees = 22,
            EventDateTime = DateTime.Now.AddDays(3).Date.AddHours(19),
            RsvpDeadline = DateTime.Now.AddDays(2).Date.AddHours(18),
            AttendeeNames = new List<string> { "Morgan", "Alex", "Riley" }
        },
        new RsvpEvent
        {
            Id = "E3",
            HostName = "Casey",
            EventName = "Posing Practice",
            Address = "9 Stage Rd, Columbia, SC",
            MaxAttendees = 12,
            CurrentAttendees = 12,
            EventDateTime = DateTime.Now.AddDays(10).Date.AddHours(16),
            RsvpDeadline = DateTime.Now.AddDays(8).Date.AddHours(12),
            AttendeeNames = new List<string> { "Casey", "Devon", "Avery" }
        }
    };

    public List<RsvpEvent> GetAllEvents() => _events.ToList();
}
