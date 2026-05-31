using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;

namespace Mercurio.Driver.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        private readonly IProviderService _providerService;

        public AppShellViewModel(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [RelayCommand]
        private async Task OpenInspectionsCommand()
        {
            string appId = string.Empty;
            string storeUrl = string.Empty;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                appId = "com.samsara.driver"; // Package ID en Android
                storeUrl = "https://play.google.com/store/apps/details?id=com.samsara.driver";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                appId = "samsara://"; // URL Scheme en iOS
                storeUrl = "https://apps.apple.com/us/app/samsara-driver/id1122606567";
            }

            try
            {
                // Try to open the application
                // On Android, MAUI will try to launch it by its Package ID. 
                // On iOS, it will try to launch it by its URL Scheme.
                bool opened = await Launcher.Default.TryOpenAsync(appId);

                if (!opened)
                {
                    // If it could not be opened, ask to download
                    bool download = await Shell.Current.DisplayAlert(
                        "App Not Found",
                        "The Samsara Driver app is not installed. Would you like to go to the store to download it?",
                        "Download",
                        "Cancel");

                    if (download)
                    {
                        await Browser.Default.OpenAsync(storeUrl, BrowserLaunchMode.SystemPreferred);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Could not complete the action.", "OK");
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SendSmsCommand()
        {
            try
            {
                // Get contact details from backend
                var provider = await _providerService.GetContactProviderAsync();

                if (provider != null && !string.IsNullOrEmpty(provider.Phone))
                {
                    // Check if the device supports sending SMS
                    if (Sms.Default.IsComposeSupported)
                    {
                        // Open the native messaging app
                        await Sms.Default.ComposeAsync(new SmsMessage("", provider.Phone));
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "SMS is not supported on this device.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Company phone number not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not open messages: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task SignOut()
        {
            AuthService _authService = new AuthService(new GpsService());
            _authService.Logout();  

            // Logout logic
            //Preferences.Clear(); 
            //await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}