using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Raphael.Driver.Platforms.Android.Services;
using Raphael.Driver.Services;

namespace Raphael.Driver
{
    [Preserve(AllMembers = true)]
    // LaunchMode SingleTop so tapping a push reuses the running activity and its extras reach
    // OnNewIntent, instead of Android stacking a second copy of the app on top of the first.
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private GpsServiceConnection _gpsServiceConnection;

        // Static property to access the service from MAUI
        public static IGpsService GpsService { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Start and bind the service
            StartAndBindGpsService();

            // The app may have been launched by a push. The request is parked here and taken
            // up once there is a Shell and a session.
            HandleNotificationIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // The app was already running: this is the tap on the push.
            Intent = intent;

            HandleNotificationIntent(intent);
        }

        /// <summary>
        /// Reads what the push left in the intent and asks for the notifications page.
        /// </summary>
        private static void HandleNotificationIntent(Intent intent)
        {
            if (intent?.GetBooleanExtra(RaphaelFirebaseMessagingService.ExtraOpenNotifications, false) != true)
                return;

            // Consumed once: without this the page reopens on every rotation, because the
            // activity is recreated with the same intent still attached.
            intent.RemoveExtra(RaphaelFirebaseMessagingService.ExtraOpenNotifications);

            NotificationRouter.RequestOpen();
        }

        private void StartAndBindGpsService()
        {
            var serviceIntent = new Intent(this, typeof(GpsServiceAndroid));

            // Simply start the service. It's not forced to the foreground yet.
            // Binding (BindService) will keep it alive as long as the app is open.
            StartService(serviceIntent);

            // Start the service so it can run indefinitely
            // StartForegroundService is required for API 26+
            /*if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(serviceIntent);
            }
            else
            {
                StartService(serviceIntent);
            }*/

            // Link the service so we can communicate with it
            _gpsServiceConnection = new GpsServiceConnection();
            BindService(serviceIntent, _gpsServiceConnection, Bind.AutoCreate);
        }

        // Class to manage the connection with the service
        [Preserve(AllMembers = true)]
        private class GpsServiceConnection : Java.Lang.Object, IServiceConnection
        {
            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                if (service is GpsServiceBinder binder)
                {
                    // We make the service instance available to the entire app
                    MainActivity.GpsService = binder.Service;
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                MainActivity.GpsService = null;
            }
        }
    }
}


/*namespace Raphael.Driver
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}*/
