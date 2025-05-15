using System.Windows.Input;
using Mercurio.Driver.Services;
using Mercurio.Driver.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel; // For ObservableObject y ObservableProperty
using CommunityToolkit.Mvvm.Input;         // For RelayCommand

namespace Mercurio.Driver.ViewModels;
public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    string _username;

    [ObservableProperty]
    string _password;

    [ObservableProperty]
    string _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordToggleIcon))] // Update icon when it changes
    bool _isPasswordMasked = true;

    [ObservableProperty]
    bool _rememberMe;

    // For show/hide password icon
    public string PasswordToggleIcon => IsPasswordMasked ? "\uf070" : "\uf06e"; // eye-slash / eye

    public LoginViewModel()
    {
             
    }

    [RelayCommand]
    private async Task Login()
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

    [RelayCommand]
    private void TogglePasswordMask()
    {
        IsPasswordMasked = !IsPasswordMasked;
    }

    [RelayCommand]
    private void GoToMilanesPageCommand()
    {
        //
    }
}
