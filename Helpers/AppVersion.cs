using Microsoft.Maui.ApplicationModel;

namespace Raphael.Driver.Helpers
{
    /// <summary>
    /// Provides application version information.
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Gets the current application version configured for the MAUI application.
        /// </summary>
        public static string Current => AppInfo.Current.VersionString;

        /// <summary>
        /// Gets the application version formatted for display.
        /// </summary>
        public static string Display => $"Version {Current}";
    }
}