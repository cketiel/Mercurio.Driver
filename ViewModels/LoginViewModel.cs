using System.Windows.Input;
using Mercurio.Driver.Services;
using Mercurio.Driver.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Mercurio.Driver.ViewModels;

public class LoginViewModel : BindableObject
{
    private readonly AuthService _authService;

    public string Username { get; set; }
    public string Password { get; set; }
    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        //_authService = authService;
        LoginCommand = new Command(async () => await LoginAsync());
    }

    private async Task LoginAsync()
    {
        // Validation
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Application.Current.MainPage.DisplayAlert("Login failed.", "Authentication", "OK");
            return;
        }
        var authService = new AuthService();
        var result = await authService.LoginAsync(new LoginRequest { Username = Username, Password = Password });

        if (result != null && result.IsSuccess)
        {
            Preferences.Set("AuthToken", result.Token);
            Preferences.Set("Username", Username);
            Preferences.Set("UserId", result.UserId);
           
            // Navigate to HomePage
            await Shell.Current.GoToAsync("//HomePage");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Login Failed", "Invalid credentials", "OK");
        }
    }
}
