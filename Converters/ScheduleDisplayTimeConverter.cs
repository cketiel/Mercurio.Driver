using System.Globalization;
using Raphael.Driver.DTOs;
using Raphael.Driver.Models;


namespace Raphael.Driver.Converters
{
    // Convert time to display (Pickup or Appt)
    public class ScheduleDisplayTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ScheduleDto schedule) return string.Empty;

            var timeToShow = schedule.EventType == ScheduleEventType.Pickup ? schedule.Pickup : schedule.Appt;
            return timeToShow?.ToString(@"hh\:mm"); // Format hh:mm
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
