namespace RSVPApp.Services;

public class AuthService
{
    // Week 2 hard-coded credentials
    private const string HardEmail = "test@example.com";
    private const string HardPassword = "Password123";
    private const string HardName = "Test User";

    public bool Validate(string email, string password, out (string name, string email) user)
    {
        user = default;

        if (string.Equals(email?.Trim(), HardEmail, StringComparison.OrdinalIgnoreCase)
            && password == HardPassword)
        {
            user = (HardName, HardEmail);
            return true;
        }

        return false;
    }
}
