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
                BuildActionsList(); // Refresh action list
            }
        }

        /// <summary>
        /// Build or rebuild the list of actions visible to the user.
        /// </summary>
        private void BuildActionsList()
        {          
            if (Event is null) return;

            Actions.Clear();

            if (!HasArrived)
            {
                // STATUS 1: The driver has not arrived yet.
                // The action is "Arrive", regardless of whether it is Pickup or Dropoff.
                Actions.Add(new EventAction { Text = "Arrive", IconGlyph = "", Command = ArriveCommand });
            }
            else // The driver has already arrived (HasArrived == true)
            {
                // STATUS 2: The driver has arrived. Now we must decide the next action.             

                // We check if a signature is required.
                // A signature is only required if it is a 'Pickup' type event AND one has not yet been captured.
                bool isSignatureRequired = Event.EventType == ScheduleEventType.Pickup && !HasSignature;

                if (isSignatureRequired)
                {
                    // CASE A: It is a Pickup and needs a signature.
                    Actions.Add(new EventAction { Text = "Passenger Signature", IconGlyph = "", Command = GoToSignaturePageCommand });
                    Actions.Add(new EventAction { Text = "Cancel Trip", IconGlyph = "", Command = CancelTripCommand });
                }
                else
                {
                    // CASE B: No signature required. This occurs if:
                    // 1. It is a 'Dropoff' type event (which skips the signature).
                    // 2. It is a 'Pickup' type event that already has a signature saved.
                    // In both cases, the next action is "Perform".
                    Actions.Add(new EventAction { Text = "Perform", IconGlyph = "", Command = PerformCommand });
                    Actions.Add(new EventAction { Text = "Cancel Trip", IconGlyph = "", Command = CancelTripCommand });
                }               
            }

            // Common actions (Call, Map, etc.) are always added at the end.
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

        [RelayCommand]
        private async Task Perform()
        {
            if (IsBusy || Event is null) return;

            IsBusy = true;
            try
            {
                
                var currentLocation = await _gpsService.GetCurrentLocationAsync();

                if (currentLocation == null)
                {
                    await Shell.Current.DisplayAlert("Location Error", "Could not get current location. Please ensure GPS is enabled and permissions are granted.", "OK");
                    return;
                }

                var eventLocation = new Location(Event.ScheduleLatitude, Event.ScheduleLongitude);
                var distanceInMiles = Location.CalculateDistance(currentLocation, eventLocation, DistanceUnits.Miles);

                
                const double ProximityThresholdInMiles = 0.1;

                if (distanceInMiles > ProximityThresholdInMiles)
                {
                    // If the distance is greater, we show the alert with the "Perform" and "Cancel" buttons.
                    bool userConfirmedPerform = await Shell.Current.DisplayAlert(
                        "Proximity Warning",
                        $"You are {distanceInMiles:F2} miles away from the destination.",
                        "Perform", // Accept button
                        "Cancel"   // Cancel button
                    );

                    if (!userConfirmedPerform)
                    {
                        return; // If the user presses "Cancel", we exit.
                    }

                    // If the user confirms, we proceed to finish.
                    await FinalizePerform(distanceInMiles);
                }
                else
                {
                    // If the distance is acceptable, we proceed directly.
                    await FinalizePerform(distanceInMiles);
                }
            }
            finally
            {
                // We make sure that the busy indicator is always disabled.
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelTrip()
        {
            if (IsBusy || Event?.TripId is null) return;

            // --- Show the first options menu (No Show / Cancel At Door) ---
            var firstChoice = await Shell.Current.DisplayActionSheet(
                "Cancel Trip", // Title
                "Cancel",      // Cancel button
                null,          // Destroy button (none)
                "No Show", "Cancel At Door");

            if (firstChoice == "No Show")
            {
                // The user selected "No Show", we proceed to show the reasons.
                await HandleNoShowCancellation();
            }
            else if (firstChoice == "Cancel At Door")
            {
                
                await Shell.Current.DisplayAlert("Not Implemented", "Cancel At Door functionality will be implemented later.", "OK");
            }
            // If the user presses "Cancel" or anything else, nothing is done.
        }
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

        private async Task FinalizePerform(double distanceInMiles)
        {            
            Event.Perform = DateTime.Now.TimeOfDay; 
            Event.PerformDist = distanceInMiles;    
            Event.Performed = true;                 
           
            var success = await _scheduleService.UpdateScheduleAsync(Event);

            if (success)
            {
                // ".." is the shell syntax to go to the previous page (TodaySchedulePage)
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // Roll back local changes if the API fails to maintain consistency
                Event.Perform = null;
                Event.PerformDist = null;
                Event.Performed = false;
                await Shell.Current.DisplayAlert("Error", "Could not save perform data. Please try again.", "OK");
            }
        }

        private async Task HandleNoShowCancellation()
        {
            // Show the second menu with the reasons for "No Show" ---
            var reason = await Shell.Current.DisplayActionSheet(
                "Reason for Cancellation", // Title
                "Cancel",                  // Cancel button
                null,                      // Destruction button
                "TIENE OTRA TRANSPORTACION",
                "NO CONTESTA EL TELEFONO NI LA PUERTA",
                "NO HA SALIDO Y LLEVO MAS DE 15 MINUTOS ESPERANDO",
                "NO QUIERE IR");

            // We check if the user selected a valid reason (it was not "Cancel" or closed the dialog)
            if (reason != null && reason != "Cancel")
            {
                IsBusy = true;
                try
                {
                    
                    bool success = await _scheduleService.CancelTripByDriverAsync(Event.TripId.Value, reason);

                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Success", "The trip has been cancelled.", "OK");
                        // Navigate back to event list
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Could not cancel the trip. Please try again.", "OK");
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}