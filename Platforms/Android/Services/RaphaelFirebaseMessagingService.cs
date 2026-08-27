using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using Raphael.Driver.Services;

// Android.OS and System both define Debug, Color and Environment. Aliased rather than
// disambiguated at every use, so the file stays readable.
using Debug = System.Diagnostics.Debug;
using AndroidColor = Android.Graphics.Color;

namespace Raphael.Driver.Platforms.Android.Services
{
    /// <summary>
    /// Receives the pushes Firebase delivers to this device.
    /// </summary>
    /// <remarks>
    /// The payload the backend sends carries identifiers only — <c>notificationId</c>,
    /// <c>businessEventCode</c> and, when the notice is about a trip, <c>tripId</c>. Nothing
    /// about the patient travels here: a push crosses Google's servers and ends up written on a
    /// lock screen where anybody standing nearby can read it. The app loads the detail when the
    /// driver opens it, already authenticated.
    /// </remarks>
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class RaphaelFirebaseMessagingService : FirebaseMessagingService
    {
        public const string ChannelId = "raphael_driver_notifications";
        public const string ExtraOpenNotifications = "raphael_open_notifications";
        public const string ExtraNotificationId = "notificationId";
        public const string ExtraTripId = "tripId";

        private const string ChannelName = "Raphael Driver";
        private const string ChannelDescription = "Trip notifications from dispatch.";

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);

            Debug.WriteLine("RaphaelFirebaseMessagingService: new FCM token.");

            // Firebase rotates tokens on its own schedule, not only at sign in. Registering it
            // here is what keeps a device reachable between shifts.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var session = ServiceHelper.GetService<NotificationSessionService>();

                    if (session is not null)
                        await session.SendTokenAsync(token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RaphaelFirebaseMessagingService: could not send the token. {ex.Message}");
                }
            });
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            try
            {
                var title = message.GetNotification()?.Title ?? "Raphael Driver";
                var body = message.GetNotification()?.Body ?? string.Empty;

                message.Data.TryGetValue(ExtraNotificationId, out var notificationId);
                message.Data.TryGetValue(ExtraTripId, out var tripId);

                Show(title, body, notificationId, tripId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RaphaelFirebaseMessagingService: {ex.Message}");
            }
        }

        private void Show(string title, string body, string? notificationId, string? tripId)
        {
            CreateChannel();

            var intent = new Intent(this, typeof(MainActivity));

            // SingleTop plus ClearTop so a tap reuses the running activity instead of stacking
            // a second copy of the app on top of the one the driver already had open.
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            intent.PutExtra(ExtraOpenNotifications, true);

            if (!string.IsNullOrEmpty(notificationId))
                intent.PutExtra(ExtraNotificationId, notificationId);

            if (!string.IsNullOrEmpty(tripId))
                intent.PutExtra(ExtraTripId, tripId);

            var pendingIntent = PendingIntent.GetActivity(
                this,
                requestCode: notificationId?.GetHashCode() ?? 0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
                // Alpha-only silhouette, never the launcher icon: Android builds the small
                // icon from the alpha channel alone, so a full colour asset renders as a
                // white square. The tint below is what gives it the brand colour.
                .SetSmallIcon(Resource.Drawable.ic_stat_notification)
                .SetColor(AndroidColor.ParseColor("#B82E49").ToArgb())
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            NotificationManagerCompat
                .From(this)
                .Notify(System.Environment.TickCount, builder.Build());
        }

        private void CreateChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var manager = (NotificationManager?)GetSystemService(NotificationService);

            if (manager is null)
                return;

            // High importance on purpose: a cancellation the driver does not see is a vehicle
            // still driving to a pickup that no longer exists.
            var channel = new NotificationChannel(
                ChannelId,
                ChannelName,
                NotificationImportance.High)
            {
                Description = ChannelDescription
            };

            manager.CreateNotificationChannel(channel);
        }
    }
}
