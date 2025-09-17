using System.Windows.Input;
using Mercurio.Driver.Services;
using Mercurio.Driver.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel; // For ObservableObject y ObservableProperty
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Mercurio.Driver.Exceptions;
using Mercurio.Driver.Views;         // For RelayCommand

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
    bool _isBusy; // To indicate charging status

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
        ErrorMessage = string.Empty; // Clean previous errors
        // Validation
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please enter username and password.";
            //Application.Current.MainPage.DisplayAlert("Login failed.", "Authentication", "OK");
            return;
        }

        IsBusy = true; // Start charging indicator

        try
        {
            var authService = new AuthService();
            var result = await authService.LoginAsync(new LoginRequest { Username = Username, Password = Password });

            if (result != null && result.IsSuccess)
            {
                Preferences.Set("AuthToken", result.Token);
                Preferences.Set("Username", Username);
                Preferences.Set("UserId", result.UserId);
                Preferences.Set("RememberMe", RememberMe);

                if (RememberMe)
                {
                    Preferences.Set("LastUsername", Username);
                }
                else
                {
                    Preferences.Remove("LastUsername");
                }

                // Navigate to HomePage 
                //await Shell.Current.GoToAsync("//HomePage");
                // Navigate to SchedulePage
                //await Shell.Current.GoToAsync("//SchedulePage");
                // Navigate to the SchedulePage. The prefix "//" resets the navigation stack
                // and set this page as the new root, restoring the menu.
                // The // prefix tells MAUI Shell: "Replace the current page (LoginPage) with this new page (SchedulePage) and make it the main page of the application."
                // Since SchedulePage is defined within your AppShell.xaml as a FlyoutItem, the Shell will automatically display the hamburger menu and navigation bar.
                await Shell.Current.GoToAsync($"//{nameof(SchedulePage)}");
                //await Shell.Current.GoToAsync($"//SchedulePage");
            }
            else
            {
                ErrorMessage = result?.Message ?? "Invalid credentials or unknown error.";
                await Application.Current.MainPage.DisplayAlert("Login Failed", "Invalid credentials", "OK");
            }
        }
        catch (ApiException apiEx)
        {
            Debug.WriteLine($"ApiException: {apiEx.Message} | Details: {apiEx.ErrorDetails} | StatusCode: {apiEx.StatusCode}");
            // More user-friendly message
            if (apiEx.Message.Contains("Connection error"))
            {
                ErrorMessage = "Could not connect to the server. Please check your connection and try again.";
            }
            else if (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized || apiEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ErrorMessage = "Incorrect username or password.";
            }
            else
            {
                ErrorMessage = $"Application error: {apiEx.Message}";
            }
            
            await Application.Current.MainPage.DisplayAlert("API error", ErrorMessage, "OK");
        }
        catch (Exception ex) // For any other unexpected exception
        {
            Debug.WriteLine($"Unexpected error in Login: {ex}");
            ErrorMessage = "An unexpected error occurred. Please try again later.";
            // Here is a good place to log the entire error (ex.ToString()) to a logging system.
            await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false; // Stop charging indicator
        }

    }

    [RelayCommand]
    private void TogglePasswordMask()
    {
        IsPasswordMasked = !IsPasswordMasked;
    }

    [RelayCommand]
    private async Task GoToMilanesPage()
    {
        try
        {
            Uri uri = new Uri("https://milanestransport.com/"); 
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            // Handle errors when opening the browser
            Debug.WriteLine($"Error opening URL: {ex.Message}");
            ErrorMessage = "The link could not be opened.";
            await Application.Current.MainPage.DisplayAlert("Error", "The link could not be opened.", "OK");
        }
    }

    // Method to load preferences when starting the view
    public void OnAppearing()
    {
        RememberMe = Preferences.Get("RememberMe", false);
        if (RememberMe)
        {
            Username = Preferences.Get("LastUsername", string.Empty);
        }
        ErrorMessage = string.Empty; // Clear errors from previous sessions
    }
}
