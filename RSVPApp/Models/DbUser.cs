using SQLite;

namespace RSVPApp.Models;

[Table("Users")]
public class DbUser
{
    [PrimaryKey, AutoIncrement]
    public int UserId { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = "";

    [MaxLength(100)]
    public string LastName { get; set; } = "";

    [MaxLength(255), Unique]
    public string Email { get; set; } = "";

    [MaxLength(255)]
    public string Password { get; set; } = "";
}
