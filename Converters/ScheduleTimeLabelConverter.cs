

using System.Globalization;
using Raphael.Driver.DTOs;
using Raphael.Driver.Models;

namespace Raphael.Driver.Converters
{
    // Convert time label (SCH TIME or APT TIME)
    public class ScheduleTimeLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ScheduleDto schedule) return string.Empty;

            return schedule.EventType == ScheduleEventType.Pickup ? "SCH TIME" : "APT TIME";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
