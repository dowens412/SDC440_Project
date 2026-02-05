namespace RSVPApp.State;

public class AppState
{
    public bool IsLoggedIn { get; private set; }
    public bool IsGuest { get; private set; }
    public string? UserName { get; private set; }
    public string? UserEmail { get; private set; }

    public void Login(string name, string email)
    {
        IsLoggedIn = true;
        IsGuest = false;
        UserName = name;
        UserEmail = email;
    }

    public void ContinueAsGuest()
    {
        IsLoggedIn = false;
        IsGuest = true;
        UserName = null;
        UserEmail = null;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        IsGuest = false;
        UserName = null;
        UserEmail = null;
    }
}
