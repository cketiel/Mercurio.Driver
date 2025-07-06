using System.Globalization;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;


namespace Mercurio.Driver.Converters
{
    public class ScheduleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ScheduleDto schedule)
                return Colors.Gray; // Default color if something goes wrong

            if (schedule.Name == "Pull-out" || schedule.Name == "Pull-in")
                return Color.FromArgb("#1c1c1c"); // Black/Dark gray

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Appointment")
                return Color.FromArgb("#2E7D32"); // Dark green

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Appointment")
                return Color.FromArgb("#C62828"); // Dark red

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Return")
                return Color.FromArgb("#1565C0"); // Dark blue

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Return")
                return Color.FromArgb("#6A1B9A"); // Purple

            return Colors.Gray; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
