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
    [QueryProperty(nameof(IsFirstEvent), "IsFirstEvent")]
    public partial class PullOutDetailPageViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IGpsService _gpsService;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotTracking))]
        private bool isTracking;

        public bool IsNotTracking => !IsTracking;

        [ObservableProperty]
        private bool _isFirstEvent;

        // Controls the visibility of the Odometer/Perform action row
        [ObservableProperty]
        private bool _isPrimaryActionVisible;

        // Properties for dynamic texts in the UI
        [ObservableProperty]
        private string _pageTitle;

        [ObservableProperty]
        private string _mapActionText;

        public PullOutDetailPageViewModel(IScheduleService scheduleService, IGpsService gpsService)
        {
            _scheduleService = scheduleService;
            _gpsService = gpsService;
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
                    //StartGpsTracking();
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
                await Shell.Current.DisplayAlert("Copied", "The address has been copied to the clipboard.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            // If there is an operation in progress, we do not allow returning to avoid inconsistent states.
            if (IsBusy)
            {
                return;
            }

            // We use ".." to navigate to the previous page in the navigation stack.
            await Shell.Current.GoToAsync("..");
        }

        // This method is automatically fired when the 'Event' property receives a value
        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                PageTitle = value.Name; // "Pull-out" o "Pull-in"
                MapActionText = $"Maps - {value.Name} Address";

                // Determine if the main action (Odometer/Perform) should be visible.
                //    - A "Pull-out" is always the first event, so it is always actionable.
                //    - A "Pull-in" is only actionable if it is the only event left in the list
                //      (which means IsFirstEvent will be true for it).
                IsPrimaryActionVisible = value.Name == "Pull-out" || (value.Name == "Pull-in" && IsFirstEvent);

                EventColor = (Color)_colorConverter.Convert(value, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);
                IsOdometerEntered = value.Odometer != null && value.Odometer > 0;
            }
            else
            {               
                EventColor = Colors.Gray;
                IsOdometerEntered = false;
            }
        }

        public void StartGpsTracking() 
        {
            if (_gpsService.IsTracking) return;

            _gpsService.StartTracking(Event.VehicleRouteId);
            IsTracking = _gpsService.IsTracking;

        }

        public void StopGpsTracking()
        {
            if (!_gpsService.IsTracking) return;

            _gpsService.StopTracking();
            IsTracking = _gpsService.IsTracking;
        }

    }
}