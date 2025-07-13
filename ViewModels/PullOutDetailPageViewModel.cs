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

        [ObservableProperty]
        private bool _isOdometerEntered;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OdometerOrPerformActionCommand))] 
        private bool _isBusy;

        public PullOutDetailPageViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteAction))]
        private async Task OdometerOrPerformAction()
        {
            if (IsOdometerEntered)
            {
                // Logic for when the user presses "Perform"
                await PerformAction();
            }
            else
            {
                // Logic for when the user presses "Odometer"
                await EnterOdometer();
            }
        }

        // Method that determines whether the command can be executed
        private bool CanExecuteAction() => !IsBusy;

        private async Task EnterOdometer()
        {
            // To avoid double clicks.
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var result = await Shell.Current.DisplayPromptAsync(
                    "Odometer",
                    "Enter Odometer Reading.",
                    "OK",
                    "Cancel",
                    keyboard: Keyboard.Numeric
                );

                if (string.IsNullOrWhiteSpace(result)) return;

                if (long.TryParse(result, out long odometerValue) && odometerValue > 0)
                {
                    Event.Odometer = odometerValue;

                    bool success = await _scheduleService.UpdateScheduleAsync(Event);

                    if (success)
                    {
                        IsOdometerEntered = true;
                        await Shell.Current.DisplayAlert("Success", "Odometer reading has been saved.", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Could not save the odometer reading. Please try again.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Invalid Input", "Please enter a valid number greater than 0 for the odometer.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PerformAction()
        {
            if (IsBusy) return; // Prevent user from clicking multiple times

            IsBusy = true;

            try
            {               
                Event.Performed = true;

                bool success = await _scheduleService.UpdateScheduleAsync(Event);

                if (success)
                {
                    // We navigate back in the navigation stack.
                    // ".." is the shell syntax for going to the previous page.
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    // If it fails, we inform the user and revert the change locally
                    // to maintain state consistency.
                    Event.Performed = false;
                    await Shell.Current.DisplayAlert("Error", "Could not perform the action. Please check your connection and try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error performing action: {ex.Message}");
                Event.Performed = false; // Rollback in case of exception
                await Shell.Current.DisplayAlert("Error", "An unexpected error occurred.", "OK");
            }
            finally
            {
                // We ensure that the 'IsBusy' status is always reset
                IsBusy = false;
            }

            // Alerta de confirmación
            //await Shell.Current.DisplayAlert("Action Performed", "The 'Perform' action has been completed successfully.", "OK");
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
                IsOdometerEntered = value.Odometer != null && value.Odometer > 0;
            }
            else
            {               
                EventColor = Colors.Gray;
                IsOdometerEntered = false;
            }
        }
      
    }
}