using System.Globalization;

namespace Mercurio.Driver.Converters
{
    public class TripTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string tripType)
            {
                return value; // Returns the original value if it is not a string
            }

            // If the value is "Appointment", replaces it with "Appt".
            // Otherwise, it returns the original value (e.g. "Return").
            if (tripType.Equals("Appointment", StringComparison.OrdinalIgnoreCase))
            {
                return "Appt";
            }

            return tripType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {          
            throw new NotImplementedException();
        }
    }
}