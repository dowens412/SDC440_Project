using SQLite;

namespace RSVPApp.Models;

[Table("Events")]
public class DbEvent
{
    [PrimaryKey, AutoIncrement]
    public int EventId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    [MaxLength(300)]
    public string Location { get; set; } = "";

    // Store as ISO string to keep it simple cross-platform
    [MaxLength(50)]
    public string EventDateTimeIso { get; set; } = "";

    // FK to Users table (host)
    public int HostUserId { get; set; }
}
