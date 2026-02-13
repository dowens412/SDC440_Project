using RSVPApp.Models;

namespace RSVPApp.Services;

public class AuthService
{
    private readonly DatabaseService _db;

    public AuthService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<(bool ok, string message, (string name, string email) user)> ValidateAsync(string email, string password)
    {
        var cleanEmail = (email ?? "").Trim();
        var cleanPassword = password ?? "";

        if (string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
            return (false, "Please enter an email and password.", default);

        var user = await _db.GetUserByEmailAsync(cleanEmail);

        if (user == null)
            return (false, "No account found for that email.", default);

        if (user.Password != cleanPassword)
            return (false, "Invalid password.", default);

        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = user.Email;

        return (true, "OK", (displayName, user.Email));
    }
}
