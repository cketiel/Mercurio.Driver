using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    public partial class TodayScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private ObservableCollection<ScheduleDto> _events;

        [ObservableProperty]
        private bool _isBusy;

        // Inyectamos el servicio a través del constructor
        public TodayScheduleViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            Events = new ObservableCollection<ScheduleDto>();
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var scheduleEvents = await _scheduleService.GetTodayScheduleAsync();

                Events.Clear();
                foreach (var ev in scheduleEvents)
                {
                    Events.Add(ev);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading events: {ex.Message}");
                // Mostrar alerta al usuario
                await Shell.Current.DisplayAlert("Error", "Could not load the schedule.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }       

    }
}