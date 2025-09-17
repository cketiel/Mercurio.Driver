using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;

namespace Mercurio.Driver.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
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