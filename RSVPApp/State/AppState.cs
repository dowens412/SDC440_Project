using RSVPApp.Models;

namespace RSVPApp.State;

public class AppState
{
    public bool IsLoggedIn => CurrentUser != null;
    public ApiUserResponse? CurrentUser { get; private set; }

    public void SetUser(ApiUserResponse user)
    {
        CurrentUser = user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}