namespace Raphael.Driver.Services
{
    /// <summary>
    /// The identifier this device is reachable by for push notifications.
    /// </summary>
    /// <remarks>
    /// Only Android has an implementation today: the app ships as an Android APK and there is
    /// no APNs setup. Elsewhere this returns nothing and the in-app channel carries everything.
    /// </remarks>
    public interface IPushTokenProvider
    {
        bool IsSupported { get; }

        /// <summary>Asks the operating system for permission to show notifications.</summary>
        Task<bool> RequestPermissionAsync();

        /// <summary>The FCM registration token, or null when there is none to be had.</summary>
        Task<string?> GetTokenAsync();
    }
}
