using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Converters;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(Event), "EventDetail")]
    [QueryProperty(nameof(SignatureSaved), "SignatureSaved")] // Receive the result of the signature page
    public partial class EventDetailPageViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IGpsService _gpsService;

        // The selected event we received from the previous page
        [ObservableProperty]
        private ScheduleDto _event;

        // The collection of actions to display in the UI
        public ObservableCollection<EventAction> Actions { get; } = new();

        // The state that controls which actions are displayed
        [ObservableProperty]
        private bool _hasArrived;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private Color _eventColor;

        private readonly ScheduleColorConverter _colorConverter = new();

        [ObservableProperty]
        private bool _hasSignature;

        // Property that is set when returning from the signature page
        [ObservableProperty]
        private bool _signatureSaved;

        public EventDetailPageViewModel(IScheduleService scheduleService, IGpsService gpsService)
        {
            _scheduleService = scheduleService;
            _gpsService = gpsService;   
        }

        // Called automatically when the 'Event' property receives a value
        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                EventColor = (Color)_colorConverter.Convert(value, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);
                // We initialize the status based on whether the arrival has already been recorded
                HasArrived = value.Arrive.HasValue;
                HasSignature = value.PassengerSignature != null; // Initialize signature status
                BuildActionsList();
                
            }
            else
            {
                EventColor = Colors.Gray;

            }
        }

        // Method that fires when SignatureSaved changes
        partial void OnSignatureSavedChanged(bool value)
        {
            if (value)
            {
                HasSignature = true;
                BuildActionsList(); // Refrescar la lista de acciones
            }
        }

        /// <summary>
        /// Build or rebuild the list of actions visible to the user.
        /// </summary>
        private void BuildActionsList()
        {
            Actions.Clear();

            if (!HasArrived)
            {
                Actions.Add(new EventAction { Text = "Arrive", IconGlyph = "", Command = ArriveCommand });
            }
            else if (HasArrived && !HasSignature) // If it has arrived but there is no signature
            {
                Actions.Add(new EventAction { Text = "Passenger Signature", IconGlyph = "", Command = GoToSignaturePageCommand });
                Actions.Add(new EventAction { Text = "Cancel Trip", IconGlyph = "", Command = CancelTripCommand });
            }
            else // If it has arrived AND there is a signature
            {
                Actions.Add(new EventAction { Text = "Perform", IconGlyph = "", Command = PerformCommand });
                Actions.Add(new EventAction { Text = "Cancel Trip", IconGlyph = "", Command = CancelTripCommand });
            }           

            // Common actions that are always visible
            Actions.Add(new EventAction { Text = "Call Customer", IconGlyph = "", Command = CallCustomerCommand });
            Actions.Add(new EventAction { Text = "Text Customer", IconGlyph = "", Command = TextCustomerCommand });
            Actions.Add(new EventAction { Text = "Maps - Appointment Address", IconGlyph = "", Command = MapsCommand });
            Actions.Add(new EventAction { Text = "Send Dispatch Message", IconGlyph = "", Command = SendDispatchMessageCommand });
        }

        [RelayCommand]
        private async Task GoToSignaturePage()
        {
            await Shell.Current.GoToAsync(nameof(SignaturePage), new Dictionary<string, object>
        {
            { "ScheduleId", Event.Id }
        });
        }

        [RelayCommand]
        private async Task Arrive()
        {
            if (IsBusy || Event is null) return;

            IsBusy = true;
            try
            {
                // GET CURRENT DEVICE LOCATION
                var currentLocation = await _gpsService.GetCurrentLocationAsync();

                if (currentLocation == null)
                {
                    // If the location could not be obtained, we inform the user and cancel the action.
                    await Shell.Current.DisplayAlert("Location Error", "Could not get current location. Please ensure GPS is enabled and permissions are granted.", "OK");
                    return;
                }
               
                var eventLocation = new Location(Event.ScheduleLatitude, Event.ScheduleLongitude);

                // CALCULATE THE DISTANCE
                var distanceInMiles = Location.CalculateDistance(currentLocation, eventLocation, DistanceUnits.Miles);

                // Distance threshold for warning
                const double ProximityThresholdInMiles = 0.1;

                if (distanceInMiles > ProximityThresholdInMiles)
                {
                    // If the distance is greater than the threshold, we show the alert with two options.
                    bool userConfirmedArrival = await Shell.Current.DisplayAlert(
                        "Proximity Warning", 
                        $"You are {distanceInMiles:F2} miles away from the destination", 
                        "Arrive", // Accept button (returns true)
                        "Cancel"  // Cancel button (returns false)
                    );

                    // If the user pressed "Cancel", we simply exit the method.
                    if (!userConfirmedArrival)
                    {
                        return; // The action is cancelled.
                    }

                    // If the user pressed "Arrive" on the alert, we go ahead and finish the arrival.
                    await FinalizeArrival(distanceInMiles, currentLocation.ToString());
                }
                else
                {
                    // If the distance is less than or equal to the threshold, no warning is displayed.
                    // We proceed directly to finalize the arrival.
                    await FinalizeArrival(distanceInMiles, currentLocation.ToString());
                }
              
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand] private void Perform() => Debug.WriteLine("Perform Tapped");
        [RelayCommand] private void CancelTrip() => Debug.WriteLine("Cancel Trip Tapped");
        [RelayCommand] private void CallCustomer() => Debug.WriteLine("Call Customer Tapped");
        [RelayCommand] private void TextCustomer() => Debug.WriteLine("Text Customer Tapped");
        [RelayCommand] private void Maps() => Debug.WriteLine("Maps Tapped");
        [RelayCommand] private void SendDispatchMessage() => Debug.WriteLine("Send Dispatch Tapped");

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
        private async Task CopyPhone()
        {
            if (Event != null && !string.IsNullOrWhiteSpace(Event.Phone))
            {
                await Clipboard.SetTextAsync(Event.Address);
                await Shell.Current.DisplayAlert("Copied", "The phone number has been copied to the clipboard.", "OK");
            }
        }

        private async Task FinalizeArrival(double distanceInMiles, string gpsArrive)
        {          
            Event.Arrive = DateTime.Now.TimeOfDay;
            Event.ArriveDist = distanceInMiles;
            Event.GPSArrive = gpsArrive;
           
            var success = await _scheduleService.UpdateScheduleAsync(Event);

            if (success)
            {               
                HasArrived = true;
                BuildActionsList(); // Update the UI
            }
            else
            {
                // Roll back local changes if the API fails to maintain consistency
                Event.Arrive = null;
                Event.ArriveDist = null;
                Event.GPSArrive = null;
                await Shell.Current.DisplayAlert("Error", "Could not save arrival time. Please try again.", "OK");
            }
        }
    }
}