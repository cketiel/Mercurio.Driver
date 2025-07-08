using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Views; 
using System.Diagnostics;
using System.Web; 

namespace Mercurio.Driver.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _runLogin;

        [ObservableProperty]
        private string _vehicleLogin;

        public ScheduleViewModel() { }

        [RelayCommand]
        private async Task GoToTodaySchedule()
        {
            if (string.IsNullOrWhiteSpace(RunLogin))
            {
                await Shell.Current.DisplayAlert("Input Required", "Please enter a Run Login.", "OK");
                return;
            }

            var navigationParameter = new Dictionary<string, object>
            {
                { "runLogin", RunLogin }
            };
         
            await Shell.Current.GoToAsync(nameof(TodaySchedulePage), navigationParameter);
        }

        [RelayCommand]
        private async Task GoToFutureSchedule()
        {
            Debug.WriteLine("Navegando al horario futuro...");
            await Shell.Current.DisplayAlert("Navegación", "Ir a Horario Futuro (lógica pendiente)", "OK");
        }
    }
}