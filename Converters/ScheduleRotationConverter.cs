using System.Globalization;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Converters
{
    public class ScheduleRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ScheduleDto schedule)
            {
                return 0; 
            }

            switch (schedule.EventType)
            {
                case ScheduleEventType.Pickup:
                    return 45; 

                case ScheduleEventType.Dropoff:
                    return -45;  

                default:
                    return 0;   
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}