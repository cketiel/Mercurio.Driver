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