using RSVPApp.Services;
using RSVPApp.State;

namespace RSVPApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly AppState _state;

    public LoginPage(AuthService auth, AppState state)
    {
        InitializeComponent();
        _auth = auth;
        _state = state;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        var result = await _auth.ValidateAsync(email, password);

        if (result.ok)
        {
            _state.Login(result.user.name, result.user.email);
            await Shell.Current.GoToAsync($"//{nameof(EventsAllPage)}");
            return;
        }

        await DisplayAlert("Login Failed", result.message, "OK");
    }

    private async void OnGuestClicked(object sender, EventArgs e)
    {
        _state.ContinueAsGuest();
        await Shell.Current.GoToAsync($"//{nameof(EventsAllPage)}");
    }
}
