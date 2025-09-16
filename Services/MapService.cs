using Mercurio.Driver.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public class MapService : IMapService
    {
        private const string DefaultMapKey = "default_map_app_scheme";

        public async Task<List<MapApp>> GetAvailableMapAppsAsync()
        {
            var availableApps = new List<MapApp>();

            #if IOS
                // On iOS, Apple Maps is always available
                availableApps.Add(new MapApp { Name = "Apple Maps", Icon = "apple_maps_icon.png", Scheme = "maps://" });

                 // We check if Google Maps can be opened
                 if (await Launcher.Default.CanOpenAsync("comgooglemaps://"))
                {
                    availableApps.Add(new MapApp { Name = "Google Maps", Icon = "google_maps_icon.png", Scheme = "comgooglemaps://" });
                }

                // We check if Waze can be opened
                if (await Launcher.Default.CanOpenAsync("waze://"))
                {
                    availableApps.Add(new MapApp { Name = "Waze", Icon = "waze_icon.png", Scheme = "waze://" });
                }
            #elif ANDROID
                // On Android, we check by package name
                if (await IsAppInstalled("com.google.android.apps.maps"))
                {
                    availableApps.Add(new MapApp { Name = "Google Maps", Icon = "google_maps_icon.png", Scheme = "com.google.android.apps.maps" });
                }
                if (await IsAppInstalled("com.waze"))
                {
                    availableApps.Add(new MapApp { Name = "Waze", Icon = "waze_icon.png", Scheme = "com.waze" });
                }
            #endif
            return availableApps;
        }

        public string GetDefaultMapAppScheme()
        {
            // If there is nothing saved, we return the scheme of the first available app
            return Preferences.Get(DefaultMapKey, GetDefaultScheme());
        }

        public void SetDefaultMapAppScheme(string scheme)
        {
            Preferences.Set(DefaultMapKey, scheme);
        }

        private string GetDefaultScheme()
        {
            #if IOS
                return "maps://"; // Apple Maps por defecto en iOS
            #elif ANDROID
                return "com.google.android.apps.maps"; // Google Maps por defecto en Android
            #else
                return string.Empty;
            #endif
        }

        public async Task LaunchNavigationAsync(double latitude, double longitude, string placemarkName)
        {
            string selectedScheme = GetDefaultMapAppScheme();

            // If there is no default app saved, we ask the user.
            if (string.IsNullOrWhiteSpace(selectedScheme))
            {
                var availableApps = await GetAvailableMapAppsAsync();

                if (!availableApps.Any())
                {
                    await Shell.Current.DisplayAlert("No Maps App", "No compatible map applications were found on your device.", "OK");
                    return;
                }

                // We create a list of app names to display in the dialog
                var appNames = availableApps.Select(app => app.Name).ToArray();

                var selectedAppName = await Shell.Current.DisplayActionSheet(
                    "Select a Map App", // Title
                    "Cancel",           // Cancel button
                    null,               // Destruction button
                    appNames);          // Options

                if (selectedAppName == null || selectedAppName == "Cancel")
                {
                    return; // User canceled
                }

                // We obtain the scheme of the selected app and save it as the new one by default
                var selectedApp = availableApps.First(app => app.Name == selectedAppName);
                selectedScheme = selectedApp.Scheme;
                SetDefaultMapAppScheme(selectedScheme);
            }

            // Now we have a scheme, either the one that was saved or the one that the user just chose.
            // We proceed to launch the corresponding app.
            // We use CultureInfo.InvariantCulture to ensure that decimal numbers use a dot. as a separator, which is what map URLs expect
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lon = longitude.ToString(CultureInfo.InvariantCulture);
            Uri navigationUri;

            switch (selectedScheme)
            {
                case "comgooglemaps://": // Google Maps on iOS
                case "com.google.android.apps.maps": // Google Maps on Android
                    navigationUri = new Uri($"https://maps.google.com/maps?daddr={lat},{lon}&directionsmode=driving");
                    break;

                case "waze://": // Waze on iOS
                case "com.waze": // Waze on Android
                    navigationUri = new Uri($"https://waze.com/ul?ll={lat},{lon}&navigate=yes");
                    break;

                case "maps://": // Apple Maps on iOS
                    navigationUri = new Uri($"http://maps.apple.com/?daddr={lat},{lon}&dirflg=d");
                    break;

                default:
                    // If for some reason the scheme is not known, we use the default MAUI launcher
                    await Map.Default.OpenAsync(latitude, longitude, new MapLaunchOptions { Name = placemarkName, NavigationMode = NavigationMode.Driving });
                    return;
            }

            try
            {
                await Launcher.Default.OpenAsync(navigationUri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error launching map app: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not open the selected map application.", "OK");
            }
        }

#if ANDROID
    private async Task<bool> IsAppInstalled(string packageName)
    {
        try
        {
            var packageManager = Android.App.Application.Context.PackageManager;
            packageManager.GetPackageInfo(packageName, Android.Content.PM.PackageInfoFlags.MatchAll);
            return true;
        }
        catch (Android.Content.PM.PackageManager.NameNotFoundException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
#endif
    }


}
