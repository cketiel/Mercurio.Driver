using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Raphael.Driver.Converters;
using Raphael.Driver.DTOs;
using Raphael.Driver.Models;
using Raphael.Driver.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Raphael.Driver.ViewModels
{
    /// <summary>
    /// One event of tomorrow's schedule, seen the day before.
    /// </summary>
    /// <remarks>
    /// Read only, with two exceptions. A driver looking at tomorrow can call and text the
    /// patient — confirming a pickup the night before is half of what prevents a no-show — and
    /// nothing else. Arriving, performing and signing belong to the day the trip runs: offered
    /// here they would either fail or, worse, work, and mark an event performed a day early.
    /// </remarks>
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class FutureDetailViewModel : ObservableObject
    {
        private readonly IPhoneDialer _phoneDialer;

        private readonly ScheduleColorConverter _colorConverter = new();

        [ObservableProperty]
        private ScheduleDto _event;

        /// <summary>Colour of the stripe down the left of each action row, by event type.</summary>
        [ObservableProperty]
        private Color _eventColor = Colors.Gray;

        public ObservableCollection<EventAction> Actions { get; } = new();

        public FutureDetailViewModel(IPhoneDialer phoneDialer)
        {
            _phoneDialer = phoneDialer;
        }

        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                EventColor = (Color)_colorConverter.Convert(
                    value,
                    typeof(Color),
                    null,
                    System.Globalization.CultureInfo.CurrentCulture);

                BuildActionsList();
            }
            else
            {
                EventColor = Colors.Gray;
            }
        }

        /// <summary>
        /// The two actions a future event offers: call the patient, and text them.
        /// </summary>
        /// <remarks>
        /// Maps and Send Dispatch Message used to be here too. Navigating to an address the
        /// driver is not going to today is an invitation to drive there a day early, and the
        /// dispatch message was never implemented: it wrote a line to the debug log and looked
        /// to the driver like a message that had been sent.
        /// </remarks>
        private void BuildActionsList()
        {
            Actions.Clear();
           
            Actions.Add(new EventAction { Text = "Call Customer", IconGlyph = "", Command = CallCustomerCommand });
            Actions.Add(new EventAction { Text = "Text Customer", IconGlyph = "", Command = TextCustomerCommand });
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