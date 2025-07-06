using System.Globalization;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Converters
{
    public class ScheduleArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ScheduleDto schedule)
            {
                return string.Empty; // Do not show anything if the data is not correct
            }

            // Select FontAwesome icon code based on event type
            switch (schedule.EventType)
            {
                case ScheduleEventType.Pickup:
                    return "\uf062"; //  "\uf062"(up) 
                case ScheduleEventType.Dropoff:
                    return "\uf063"; //  "\uf063"(down) 
                default:
                    return string.Empty; //Don't show arrow for other types
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}