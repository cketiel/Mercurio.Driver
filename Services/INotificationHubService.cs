namespace Raphael.Driver.Services
{
    /// <summary>
    /// Live connection to the notification hub, so a notice reaches the driver while the app
    /// is open without waiting for a refresh.
    /// </summary>
    public interface INotificationHubService
    {
        bool IsConnected { get; }

        /// <summary>Opens the connection with the session token. Safe to call twice.</summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>Closes it. Called on sign out, before the token is wiped.</summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
