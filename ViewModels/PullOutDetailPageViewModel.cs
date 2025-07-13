using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using System.Diagnostics;
using Mercurio.Driver.Converters;
using Mercurio.Driver.Services;

namespace Mercurio.Driver.ViewModels
{
    // We use QueryProperty to receive the ScheduleDto object during navigation
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class PullOutDetailPageViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private ScheduleDto _event;

        [ObservableProperty]
        private Color _eventColor;

        private readonly ScheduleColorConverter _colorConverter = new();

        public PullOutDetailPageViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [RelayCommand]
        private async Task GoToOdometer()
        {
            // Shows the text entry dialog
            var result = await Shell.Current.DisplayPromptAsync(
                "Odometer",
                "Enter Odometer Reading.",
                "OK",
                "Cancel",
                keyboard: Keyboard.Numeric // This displays a numeric keypad to the user
            );

            // The user pressed "Cancel" or left the field empty
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            // We validate that the input is a number
            if (long.TryParse(result, out long odometerValue))
            {
                // The object in the ViewModel is updated
                Event.Odometer = odometerValue;

                // Service is called to save changes to the API
                bool success = await _scheduleService.UpdateScheduleAsync(Event);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Odometer reading has been saved.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Could not save the odometer reading. Please try again.", "OK");
                }
            }
            else
            {
                // The user entered something that is not a valid number
                await Shell.Current.DisplayAlert("Invalid Input", "Please enter a valid number for the odometer.", "OK");
            }

            // TODO: Implementar navegación a la página del Odómetro
            /*await Shell.Current.DisplayAlert("Función no implementada", "La navegación al odómetro aún no está disponible.", "OK");
            Debug.WriteLine("Navegando a la página del Odómetro...");*/
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