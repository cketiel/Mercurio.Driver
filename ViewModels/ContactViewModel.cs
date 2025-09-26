using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Mercurio.Driver.DTOs;
using Mercurio.Driver.Services;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mercurio.Driver.ViewModels
{
    public partial class ContactViewModel : ObservableObject
    {
        private readonly IProviderService _providerService;
        private readonly IPhoneDialer _phoneDialer;
        private readonly IMapService _mapService;

        [ObservableProperty]
        private ProviderDto _provider;

        [ObservableProperty]
        private bool _isBusy;

        public ContactViewModel(IProviderService providerService, IPhoneDialer phoneDialer, IMapService mapService)
        {
            _providerService = providerService;
            _phoneDialer = phoneDialer;
            _mapService = mapService;
            _provider = new ProviderDto(); 
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                Provider = await _providerService.GetContactProviderAsync() ?? new ProviderDto { Name = "Not available" };
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (string.IsNullOrWhiteSpace(Provider?.Phone))
            {
                await Shell.Current.DisplayAlert("Invalid", "The phone number cannot be empty.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var success = await _providerService.UpdateContactProviderAsync(Provider);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Contact information has been updated.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Could not update information.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CallProviderAsync()
        {
            if (string.IsNullOrWhiteSpace(Provider?.Phone))
            {
                await Shell.Current.DisplayAlert("Not available", "There is no phone number to call.", "OK");
                return;
            }

            if (await Shell.Current.DisplayAlert("Confirm", $"Do you want to call the office?\n{Provider.Phone}", "Call", "Cancel"))
            {
                try
                {
                    if (_phoneDialer.IsSupported)
                        _phoneDialer.Open(Provider.Phone);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error when trying to call: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "The call could not be started.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task NavigateToProviderAsync()
        {
            if (Provider?.Latitude == null || Provider?.Longitude == null || string.IsNullOrWhiteSpace(Provider.Address))
            {
                await Shell.Current.DisplayAlert("Not available", "There is no valid address to navigate to.", "OK");
                return;
            }
            await _mapService.LaunchNavigationAsync(Provider.Latitude.Value, Provider.Longitude.Value, Provider.Address);
        }
    }
}