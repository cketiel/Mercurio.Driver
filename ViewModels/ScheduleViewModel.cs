using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        [ObservableProperty]
        string _runLogin;

        [ObservableProperty]
        string _vehicleLogin;

        public ScheduleViewModel()
        {
            // Constructor puede estar vacío por ahora
        }

        [RelayCommand]
        private async Task GoToTodaySchedule()
        {
            // Aquí irá la lógica para navegar al horario de hoy
            Debug.WriteLine("Navegando al horario de hoy...");
            await Shell.Current.DisplayAlert("Navegación", "Ir a Horario de Hoy (lógica pendiente)", "OK");
        }

        [RelayCommand]
        private async Task GoToFutureSchedule()
        {
            // Aquí irá la lógica para navegar a los horarios futuros
            Debug.WriteLine("Navegando al horario futuro...");
            await Shell.Current.DisplayAlert("Navegación", "Ir a Horario Futuro (lógica pendiente)", "OK");
        }
    }
}