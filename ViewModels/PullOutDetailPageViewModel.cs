using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using System.Diagnostics;
using Mercurio.Driver.Converters;

namespace Mercurio.Driver.ViewModels
{
    // We use QueryProperty to receive the ScheduleDto object during navigation
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class PullOutDetailPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ScheduleDto _event;

        [ObservableProperty]
        private Color _eventColor;

        private readonly ScheduleColorConverter _colorConverter = new();

        public PullOutDetailPageViewModel()
        {
        }

        [RelayCommand]
        private async Task GoToOdometer()
        {
            // TODO: Implementar navegación a la página del Odómetro
            await Shell.Current.DisplayAlert("Función no implementada", "La navegación al odómetro aún no está disponible.", "OK");
            Debug.WriteLine("Navegando a la página del Odómetro...");
        }

        [RelayCommand]
        private async Task GoToMaps()
        {
            // TODO: Implementar la apertura de mapas con la dirección del evento
            if (Event != null && !string.IsNullOrWhiteSpace(Event.Address))
            {
                await Shell.Current.DisplayAlert("Función no implementada", $"Abrir mapas con la dirección: {Event.Address}", "OK");
                Debug.WriteLine($"Abriendo mapas para la dirección: {Event.Address}");
            }
        }

        [RelayCommand]
        private async Task CopyAddress()
        {
            if (Event != null && !string.IsNullOrWhiteSpace(Event.Address))
            {
                await Clipboard.SetTextAsync(Event.Address);
                await Shell.Current.DisplayAlert("Copiado", "La dirección ha sido copiada al portapapeles.", "OK");
            }
        }

        // This method is automatically fired when the 'Event' property receives a value
        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {              
                EventColor = (Color)_colorConverter.Convert(value, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {               
                EventColor = Colors.Gray;
            }
        }
      
    }
}