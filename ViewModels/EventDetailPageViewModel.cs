using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models; 
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mercurio.Driver.Converters;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class EventDetailPageViewModel : ObservableObject
    {
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

        public EventDetailPageViewModel()
        {
            
            
        }

        // Called automatically when the 'Event' property receives a value
        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                EventColor = (Color)_colorConverter.Convert(value, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);
                // We initialize the status based on whether the arrival has already been recorded
                HasArrived = value.Arrive.HasValue;
                BuildActionsList();
                
            }
            else
            {
                EventColor = Colors.Gray;

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
            else
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
        private async Task Arrive()
        {
            if (IsBusy) return;

            IsBusy = true;
            Debug.WriteLine("Running the Arrive action...");
           
            Event.Arrive = TimeSpan.FromHours(DateTime.Now.Hour); 
          
            //var success = await _scheduleService.UpdateScheduleAsync(Event);

            
            // await Task.Delay(500); // Simula una llamada de red
           
            HasArrived = true;
            BuildActionsList(); // Rebuilds the action list to show "Perform" and "Cancel"

            IsBusy = false;
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
    }
}