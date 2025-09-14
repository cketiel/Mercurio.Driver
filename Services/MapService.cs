using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mercurio.Driver.Models;

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
