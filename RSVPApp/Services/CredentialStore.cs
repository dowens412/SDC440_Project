using Microsoft.Maui.Storage;

namespace RSVPApp.Services;

public interface ICredentialStore
{
    Task SaveAsync(string email, string password);
    Task<(string Email, string Password)?> GetAsync();
    Task ClearAsync();
}

public sealed class CredentialStore : ICredentialStore
{
    private const string EmailKey = "rsvp_email";
    private const string PasswordKey = "rsvp_password";

    public Task SaveAsync(string email, string password)
    {
        Preferences.Set(EmailKey, email);
        Preferences.Set(PasswordKey, password);
        return Task.CompletedTask;
    }

    public Task<(string Email, string Password)?> GetAsync()
    {
        var email = Preferences.Get(EmailKey, string.Empty);
        var pass = Preferences.Get(PasswordKey, string.Empty);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            return Task.FromResult<(string, string)?>(null);

        return Task.FromResult<(string, string)?>((email, pass));
    }

    public Task ClearAsync()
    {
        Preferences.Remove(EmailKey);
        Preferences.Remove(PasswordKey);
        return Task.CompletedTask;
    }
}