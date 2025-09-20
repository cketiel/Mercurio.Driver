using System.Globalization;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Converters
{
    public class ScheduleHistoryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ¡CAMBIO CLAVE! Ahora trabajamos con ScheduleHistoryDto
            if (value is not ScheduleHistoryDto schedule)
                return Colors.Gray;

            // Para eventos cancelados, el diseño es diferente (negro), lo manejamos en el XAML.
            // Para eventos completados:
            if (schedule.Name == "Pull-out" || schedule.Name == "Pull-in")
                return Colors.White; // La línea de tiempo para Pull-out en la imagen es blanca.

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Appointment")
                return Color.FromArgb("#2E7D32"); // Verde

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Appointment")
                return Color.FromArgb("#C62828"); // Rojo

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Return")
                return Color.FromArgb("#1565C0"); // Azul

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Return")
                return Color.FromArgb("#6A1B9A"); // Púrpura

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}