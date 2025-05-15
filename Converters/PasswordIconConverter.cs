using System.Globalization;

namespace Mercurio.Driver.Converters
{
    public class PasswordIconConverter : IValueConverter
    {
        // FontAwesome Unicode characters
        private const string EyeIcon = "\uf06e"; // fa-eye
        private const string EyeSlashIcon = "\uf070"; // fa-eye-slash

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? EyeSlashIcon : EyeIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed
            throw new NotImplementedException();
        }
    }
}