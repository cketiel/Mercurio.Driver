using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Views;

namespace Mercurio.Driver.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task GoToSchedule()
        {
            // Close the flyout menu and navigate to the SchedulePage
            await Shell.Current.GoToAsync($"//{nameof(SchedulePage)}");
        }

        [RelayCommand]
        private async Task SignOut()
        {
            // Logout logic
            Preferences.Clear(); 
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}