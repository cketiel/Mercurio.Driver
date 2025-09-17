using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
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

        [ObservableProperty]
        private bool _isBusy;

        public ScheduleViewModel() { }

        [RelayCommand]
        private async Task GoToTodaySchedule()
        {
            if (IsBusy) return; // Avoid multiple clicks

            if (string.IsNullOrWhiteSpace(RunLogin))
            {
                await Shell.Current.DisplayAlert("Input Required", "Please enter a Run Login.", "OK");
                return;
            }

            try
            {
                IsBusy = true; 

                var navigationParameter = new Dictionary<string, object>
                {
                    { "runLogin", RunLogin }
                };

                await Shell.Current.GoToAsync(nameof(TodaySchedulePage), navigationParameter);
            }
            catch (Exception ex)
            {               
                Debug.WriteLine($"Error while browsing: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "The schedule page could not be opened.", "OK");
            }
            finally
            {
                IsBusy = false; 
            }      
        }

        [RelayCommand]
        private async Task GoToFutureSchedule()
        {
            Debug.WriteLine("Navegando al horario futuro...");
            await Shell.Current.DisplayAlert("Navegación", "Ir a Horario Futuro (lógica pendiente)", "OK");
        }
    }
}