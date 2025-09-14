using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Models;
using Mercurio.Driver.Services;
using System.Collections.ObjectModel;

namespace Mercurio.Driver.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IMapService _mapService;
        public ObservableCollection<MapApp> AvailableMaps { get; } = new();

        public SettingsViewModel(IMapService mapService)
        {
            _mapService = mapService;
        }

        [RelayCommand]
        private async Task LoadMapAppsAsync()
        {
            if (AvailableMaps.Any()) return;

            // --- CÓDIGO DE PRUEBA PARA VERIFICAR LA UI ---           
            /*AvailableMaps.Add(new MapApp { Name = "Google Maps", Icon = "google_maps_icon.png", IsSelected = true, Scheme = "comgooglemaps://" });
            AvailableMaps.Add(new MapApp { Name = "Waze", Icon = "waze_icon.png", IsSelected = false, Scheme = "waze://" });
            AvailableMaps.Add(new MapApp { Name = "Apple Maps", Icon = "apple_maps_icon.png", IsSelected = false, Scheme = "maps://" });*/

            
            
            var apps = await _mapService.GetAvailableMapAppsAsync();
            var defaultScheme = _mapService.GetDefaultMapAppScheme();

            foreach (var app in apps)
            {
                if (app.Scheme == defaultScheme)
                {
                    app.IsSelected = true;
                }
                AvailableMaps.Add(app);
            }
            
        }

        [RelayCommand]
        private void SelectMap(MapApp selectedApp)
        {
            // Save to prevent it from being executed if the user touches the already selected item.
            if (selectedApp == null || selectedApp.IsSelected)
            {
                return;
            }

            // Deselect the previous item in the data model
            var previouslySelected = AvailableMaps.FirstOrDefault(m => m.IsSelected);
            if (previouslySelected != null)
            {
                previouslySelected.IsSelected = false;
            }

            // Select the new one in the data model
            selectedApp.IsSelected = true;

            // Save preference 
            _mapService.SetDefaultMapAppScheme(selectedApp.Scheme);
        }
    }
}