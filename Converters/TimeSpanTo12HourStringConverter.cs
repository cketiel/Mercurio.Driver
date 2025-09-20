using System.Globalization;

namespace Mercurio.Driver.Converters
{
    public class TimeSpanTo12HourStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {          
            if (value is TimeSpan timeSpan)
            {
                
                DateTime dateTime = DateTime.Today.Add(timeSpan);
                return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            }
            
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}