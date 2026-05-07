using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Converters
{
    public class TimeSpanToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                // Convertimos el TimeSpan a un DateTime (usando la fecha de hoy como base)
                // para que los formatos "hh:mm tt" funcionen.
                return DateTime.Today.Add(ts);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
