using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;
using Mercurio.Driver.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class FutureDetailViewModel : ObservableObject
    {
        private readonly IMapService _mapService;

        [ObservableProperty]
        private ScheduleDto _event;

        public ObservableCollection<EventAction> Actions { get; } = new();

        public FutureDetailViewModel(IMapService mapService)
        {
            _mapService = mapService;
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

        [RelayCommand] private void CallCustomer() => Debug.WriteLine("Call Customer Tapped");
        [RelayCommand] private void TextCustomer() => Debug.WriteLine("Text Customer Tapped");
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
            if (Event != null && !string.IsNullOrWhiteSpace(Event.Phone))
            {
                await Clipboard.SetTextAsync(Event.Address);
                await Shell.Current.DisplayAlert("Copied", "The phone number has been copied to the clipboard.", "OK");
            }
        }
    }
}