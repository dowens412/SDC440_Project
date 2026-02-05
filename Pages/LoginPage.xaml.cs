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

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validation", "Please enter an email and password.", "OK");
            return;
        }

        if (_auth.Validate(email, password, out var user))
        {
            _state.Login(user.name, user.email);
            await Shell.Current.GoToAsync($"//{nameof(EventsAllPage)}");
            return;
        }

        await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
    }

    private async void OnGuestClicked(object sender, EventArgs e)
    {
        _state.ContinueAsGuest();
        await Shell.Current.GoToAsync($"//{nameof(EventsAllPage)}");
    }
}
