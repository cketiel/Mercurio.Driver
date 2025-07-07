using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Views;

namespace Mercurio.Driver.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        [RelayCommand]
        async Task SignOut()
        {
            // Logout logic
            Preferences.Clear(); 
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}