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
        private readonly ISessionManagerService _sessionManager;

        [ObservableProperty]
        private bool _isHistoryAvailable;

        [ObservableProperty]
        private ObservableCollection<ScheduleDto> _events;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _runLogin;

        [ObservableProperty]
        private bool _hasEvents;

        [ObservableProperty]
        private bool _showNoEventsMessage;

        public TodayScheduleViewModel(IScheduleService scheduleService, ISessionManagerService sessionManager)
        {
            _scheduleService = scheduleService;
            _sessionManager = sessionManager;
            Events = new ObservableCollection<ScheduleDto>();
        }
        private void UpdateUiState()
        {
            HasEvents = Events.Any();
            ShowNoEventsMessage = !HasEvents && !IsBusy;
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(RunLogin))
                return;

            try
            {
                IsBusy = true;
                UpdateUiState();

                var pendingEvents = await _scheduleService.GetPendingSchedulesByRunAsync(RunLogin, DateTime.Today);
                // var pendingEvents = allScheduleEvents.Where(ev => !ev.Performed).ToList();

                Events.Clear();
                foreach (var ev in pendingEvents)
                {
                    Events.Add(ev);
                }

                //SessionManagerService _sessionManager = new SessionManagerService(new GpsService());
                //await _sessionManager.CheckAndResumeGpsTrackingAsync(Events);

                var historyCount = await _scheduleService.GetScheduleHistoryCountAsync(RunLogin, DateTime.Today);
                IsHistoryAvailable = historyCount > 0;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading events: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not load the schedule.", "OK");
            }
            finally
            {
                IsBusy = false;
                UpdateUiState();
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

            // The first event in the current list is the only "active" one.
            // We compare the selected event with the first element of the collection.
            bool isFirstEvent = Events.FirstOrDefault() == selectedEvent;
            
            if (selectedEvent.Name == "Pull-in" || selectedEvent.Name == "Pull-out")
            {
                // Navegamos pasando el objeto como parámetro
                await Shell.Current.GoToAsync(nameof(PullOutDetailPage), new Dictionary<string, object>
                {
                    { "EventDetail", selectedEvent },
                    { "IsFirstEvent", isFirstEvent }
                });
            }
            else
            {
                
                await Shell.Current.GoToAsync(nameof(EventDetailPage), new Dictionary<string, object>
                {
                    { "EventDetail", selectedEvent },
                    { "IsFirstEvent", isFirstEvent }
                });
            }
        }

        [RelayCommand]
        private async Task GoToHistory()
        {
            if (string.IsNullOrWhiteSpace(RunLogin)) return;

            // Navegamos a la HistoryPage, pasando el RunLogin como parámetro
            await Shell.Current.GoToAsync(nameof(HistoryPage), new Dictionary<string, object>
    {
        { "runLogin", RunLogin }
    });
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