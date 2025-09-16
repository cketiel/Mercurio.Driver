// In ViewModels/EventDetailPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models; // Asegúrate de incluir el nuevo modelo
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(Event), "EventDetail")]
    public partial class EventDetailPageViewModel : ObservableObject
    {
        // El evento seleccionado que recibimos de la página anterior
        [ObservableProperty]
        private ScheduleDto _event;

        // La colección de acciones que se mostrará en la UI
        public ObservableCollection<EventAction> Actions { get; } = new();

        // El estado que controla qué acciones se muestran
        [ObservableProperty]
        private bool _hasArrived;

        [ObservableProperty]
        private bool _isBusy;

        public EventDetailPageViewModel()
        {
            // Constructor vacío es necesario para la inyección de dependencias
        }

        // Se llama automáticamente cuando la propiedad 'Event' recibe un valor
        partial void OnEventChanged(ScheduleDto value)
        {
            if (value != null)
            {
                // Inicializamos el estado basado en si ya se ha registrado la llegada
                HasArrived = value.Arrive.HasValue;
                BuildActionsList();
            }
        }

        /// <summary>
        /// Construye o reconstruye la lista de acciones visibles para el usuario.
        /// </summary>
        private void BuildActionsList()
        {
            Actions.Clear();

            if (!HasArrived)
            {
                Actions.Add(new EventAction { Text = "Arrive", IconGlyph = "", Command = ArriveCommand });
            }
            else
            {
                Actions.Add(new EventAction { Text = "Perform", IconGlyph = "", Command = PerformCommand });
                Actions.Add(new EventAction { Text = "Cancel Trip", IconGlyph = "", Command = CancelTripCommand });
            }

            // Acciones comunes que siempre están visibles
            Actions.Add(new EventAction { Text = "Call Customer", IconGlyph = "", Command = CallCustomerCommand });
            Actions.Add(new EventAction { Text = "Text Customer", IconGlyph = "", Command = TextCustomerCommand });
            Actions.Add(new EventAction { Text = "Maps - Appointment Address", IconGlyph = "", Command = MapsCommand });
            Actions.Add(new EventAction { Text = "Send Dispatch Message", IconGlyph = "", Command = SendDispatchMessageCommand });
        }

        [RelayCommand]
        private async Task Arrive()
        {
            if (IsBusy) return;

            IsBusy = true;
            Debug.WriteLine("Ejecutando la acción Arrive...");

            // --- LÓGICA DE LA ACCIÓN ARRIVE ---
            // 1. Actualizar el objeto localmente
            Event.Arrive = TimeSpan.FromHours(DateTime.Now.Hour); // TODO: Usar la hora real

            // 2. TODO: Llamar a un servicio para persistir este cambio en el backend
            // var success = await _scheduleService.UpdateScheduleAsync(Event);

            // 3. Simular éxito por ahora
            await Task.Delay(500); // Simula una llamada de red

            // 4. Actualizar el estado de la UI
            HasArrived = true;
            BuildActionsList(); // Reconstruye la lista de acciones para mostrar "Perform" y "Cancel"

            IsBusy = false;
        }

        // --- Comandos de marcador de posición para las otras acciones ---
        [RelayCommand] private void Perform() => Debug.WriteLine("Perform Tapped");
        [RelayCommand] private void CancelTrip() => Debug.WriteLine("Cancel Trip Tapped");
        [RelayCommand] private void CallCustomer() => Debug.WriteLine("Call Customer Tapped");
        [RelayCommand] private void TextCustomer() => Debug.WriteLine("Text Customer Tapped");
        [RelayCommand] private void Maps() => Debug.WriteLine("Maps Tapped");
        [RelayCommand] private void SendDispatchMessage() => Debug.WriteLine("Send Dispatch Tapped");
    }
}