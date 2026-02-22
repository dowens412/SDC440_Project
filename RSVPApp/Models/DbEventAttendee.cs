using SQLite;

namespace RSVPApp.Models;

public class DbEventAttendee
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string EventId { get; set; } = "";

    [Indexed]
    public string UserId { get; set; } = "";

    public string UserName { get; set; } = "";
}