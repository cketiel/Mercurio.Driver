using System.Diagnostics;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// Takes the driver to the notifications page when they tap a push.
    /// </summary>
    /// <remarks>
    /// A push can arrive with the app closed. In that case Android launches the activity and
    /// the request gets here before there is a Shell to navigate, and often before there is a
    /// session at all — the driver still has to sign in. So the request is parked and consumed
    /// once both exist.
    /// </remarks>
    public static class NotificationRouter
    {
        private static bool _pending;

        public static bool HasPending => _pending;

        /// <summary>
        /// Called from the platform layer when a push is tapped.
        /// </summary>
        public static void RequestOpen()
        {
            _pending = true;

            MainThread.BeginInvokeOnMainThread(async () => await TryConsumeAsync());
        }

        /// <summary>
        /// Navigates if it can. Called again after sign in, when the conditions are finally met.
        /// </summary>
        public static async Task TryConsumeAsync()
        {
            if (!_pending)
                return;

            if (Shell.Current is null)
                return;

            // No session yet: the driver is about to see the login page. The request waits.
            if (string.IsNullOrWhiteSpace(Preferences.Get("AuthToken", string.Empty)))
                return;

            _pending = false;

            try
            {
                await Shell.Current.GoToAsync("//NotificationsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NotificationRouter: could not navigate. {ex.Message}");
            }
        }

        /// <summary>Drops a parked request. Called on sign out.</summary>
        public static void Clear() => _pending = false;
    }
}
