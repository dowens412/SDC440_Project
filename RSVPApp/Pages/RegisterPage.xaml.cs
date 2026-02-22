using RSVPApp.Models;
using RSVPApp.Services;

namespace RSVPApp.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;

    public RegisterPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "";

        try
        {
            var name = NameEntry.Text?.Trim() ?? "";
            var email = EmailEntry.Text?.Trim() ?? "";
            var phone = PhoneEntry.Text?.Trim() ?? "";
            var pass = PasswordEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(pass))
            {
                StatusLabel.Text = "Fill in all fields.";
                return;
            }

            await _auth.RegisterAsync(new ApiUserRegisterRequest(name, email, pass, phone));

            // After register, go back and let them login
            await DisplayAlert("Account Created", "Now login with your email and password.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}