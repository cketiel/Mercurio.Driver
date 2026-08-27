using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Raphael.Driver.DTOs;
using Raphael.Driver.Models;
using Raphael.Driver.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Raphael.Driver.ViewModels
{
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class FutureDetailViewModel : ObservableObject
    {
        private readonly IMapService _mapService;
        private readonly IPhoneDialer _phoneDialer;

        [ObservableProperty]
        private ScheduleDto _event;

        public ObservableCollection<EventAction> Actions { get; } = new();

        public FutureDetailViewModel(IMapService mapService, IPhoneDialer phoneDialer)
        {
            _mapService = mapService;
            _phoneDialer = phoneDialer;
        }

        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                BuildActionsList();
            }
        }

        private void BuildActionsList()
        {
            Actions.Clear();
           
            Actions.Add(new EventAction { Text = "Call Customer", IconGlyph = "", Command = CallCustomerCommand });
            Actions.Add(new EventAction { Text = "Text Customer", IconGlyph = "", Command = TextCustomerCommand });
            string mapActionText = Event.TripType == "Appointment" ? "Maps - Appointment Address" : "Maps - Return Address";
            Actions.Add(new EventAction { Text = mapActionText, IconGlyph = "", Command = MapsCommand });
            Actions.Add(new EventAction { Text = "Send Dispatch Message", IconGlyph = "", Command = SendDispatchMessageCommand });
        }

        /// <summary>
        /// Calls the patient of this future trip.
        /// </summary>
        /// <remarks>
        /// Same flow as <c>EventDetailPageViewModel.CallCustomer</c>: the number is confirmed
        /// before dialling. A driver taps this on a phone mounted on a windscreen, and a
        /// misplaced tap that dials a patient straight away is a call nobody meant to make.
        /// </remarks>
        [RelayCommand]
        private async Task CallCustomer()
        {
            var phone = CustomerPhone();

            if (phone is null)
            {
                await Shell.Current.DisplayAlert(
                    "Not available",
                    "There is no contact phone number for this event.",
                    "OK");
                return;
            }

            try
            {
                var wantsToCall = await Shell.Current.DisplayAlert(
                    "Confirm call",
                    $"Do you want to call this number?\n{phone}",
                    "Call",
                    "Cancel");

                if (!wantsToCall)
                    return;

                if (!_phoneDialer.IsSupported)
                {
                    await Shell.Current.DisplayAlert("Not supported", "This device cannot make calls.", "OK");
                    return;
                }

                _phoneDialer.Open(phone);
            }
            catch (FeatureNotSupportedException)
            {
                await Shell.Current.DisplayAlert("Not supported", "Calling functionality is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to call: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An unexpected error occurred while trying to make the call.", "OK");
            }
        }

        /// <summary>
        /// Opens the phone's messaging app with the patient's number already filled in.
        /// </summary>
        /// <remarks>
        /// It composes but does not send: the driver writes the text and presses send in their
        /// own messaging app. Nothing is written from here, and no message body is prefilled —
        /// what a driver has to say to a patient is theirs to write.
        /// </remarks>
        [RelayCommand]
        private async Task TextCustomer()
        {
            var phone = CustomerPhone();

            if (phone is null)
            {
                await Shell.Current.DisplayAlert(
                    "Not available",
                    "There is no contact phone number for this event.",
                    "OK");
                return;
            }

            try
            {
                if (!Sms.Default.IsComposeSupported)
                {
                    await Shell.Current.DisplayAlert("Not supported", "SMS is not supported on this device.", "OK");
                    return;
                }

                await Sms.Default.ComposeAsync(new SmsMessage(string.Empty, phone));
            }
            catch (FeatureNotSupportedException)
            {
                await Shell.Current.DisplayAlert("Not supported", "SMS is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to text the customer: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not open the messaging app.", "OK");
            }
        }

        /// <summary>
        /// The number to reach the patient on, or null when there is none.
        /// </summary>
        /// <remarks>
        /// <c>CustomerPhone</c> is the number dispatch keeps on the customer record and the one
        /// the driver can correct from the event detail, so it wins. <c>Phone</c> is what came
        /// with the schedule row and is the fallback.
        /// </remarks>
        private string? CustomerPhone()
        {
            if (Event is null)
                return null;

            if (!string.IsNullOrWhiteSpace(Event.CustomerPhone))
                return Event.CustomerPhone;

            return string.IsNullOrWhiteSpace(Event.Phone)
                ? null
                : Event.Phone;
        }

        [RelayCommand] private void SendDispatchMessage() => Debug.WriteLine("Send Dispatch Tapped");

        [RelayCommand]
        private async Task Maps()
        {
            await _mapService.LaunchNavigationAsync(Event.ScheduleLatitude, Event.ScheduleLongitude, Event.Address);
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
        private async Task CopyPhone()
        {
            // Copied the address, not the phone. The alert said "phone number", so the driver
            // pasted a street into a dialler and had no reason to suspect it.
            var phone = CustomerPhone();

            if (phone is not null)
            {
                await Clipboard.SetTextAsync(phone);
                await Shell.Current.DisplayAlert("Copied", "The phone number has been copied to the clipboard.", "OK");
            }
        }
    }
}