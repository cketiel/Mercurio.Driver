﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(RunLogin), "runLogin")]
    public partial class TodayScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private ObservableCollection<ScheduleDto> _events;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _runLogin;

        public TodayScheduleViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            Events = new ObservableCollection<ScheduleDto>();
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(RunLogin))
                return;

            try
            {
                IsBusy = true;                             
                var scheduleEvents = await _scheduleService.GetSchedulesByRunAsync(RunLogin, DateTime.Today);

                Events.Clear();
                foreach (var ev in scheduleEvents)
                {
                    Events.Add(ev);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading events: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not load the schedule.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenMenu()
        {
            Shell.Current.FlyoutIsPresented = true;
        }

        [RelayCommand]
        private async Task SelectEvent(ScheduleDto selectedEvent)
        {
            if (selectedEvent == null)
                return;

            // Verificamos si es un evento que requiere la página de detalles
            if (selectedEvent.Name == "Pull-in" || selectedEvent.Name == "Pull-out")
            {
                // Navegamos pasando el objeto como parámetro
                await Shell.Current.GoToAsync(nameof(PullOutDetailPage), new Dictionary<string, object>
                {
                    { "EventDetail", selectedEvent }
                });
            }
            else
            {
                // TODO: Implementar la navegación para eventos normales (Pickup/Dropoff)
                Debug.WriteLine($"Evento normal seleccionado: {selectedEvent.Patient}");
                // Por ejemplo: await Shell.Current.GoToAsync(nameof(NormalEventDetailPage), ...);
            }
        }

        // Automatic loading when RunLogin is set
        partial void OnRunLoginChanged(string value)
        {
            // When the RunLogin property receives a value, events are loaded
            if (!string.IsNullOrWhiteSpace(value))
            {
                _ = LoadEventsAsync();
            }
        }
    }
}