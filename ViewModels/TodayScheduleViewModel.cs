using CommunityToolkit.Mvvm.ComponentModel;
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
                var pendingEvents = await _scheduleService.GetPendingSchedulesByRunAsync(RunLogin, DateTime.Today);
                // var pendingEvents = allScheduleEvents.Where(ev => !ev.Performed).ToList();

                Events.Clear();
                foreach (var ev in pendingEvents)
                {
                    Events.Add(ev);
                }

                SessionManagerService _sessionManager = new SessionManagerService(new GpsService());
                //await _sessionManager.CheckAndResumeGpsTrackingAsync(Events);
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