using Raphael.Driver.DTOs;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// The driver's notification inbox on the API.
    /// </summary>
    /// <remarks>
    /// Everything hangs off <c>api/driver/notifications</c>. Note that view, unview and
    /// acknowledge take the <b>recipient row</b> identifier
    /// (<see cref="NotificationDto.RecipientRecordId"/>), not the notification identifier:
    /// one notification carries a row per audience, and marking the wrong one reads somebody
    /// else's copy.
    /// </remarks>
    public interface INotificationApiService
    {
        /// <summary>
        /// The driver's inbox, or <c>null</c> when the call did not come back.
        /// </summary>
        /// <remarks>
        /// ⚠️ Null and empty mean very different things and must not be collapsed. An empty
        /// list is "you have no notifications"; null is "we could not ask". A 403 — which is
        /// what the API answers a user whose role is not in <c>DriverRoleIds</c> — used to
        /// arrive as an empty list, so a deployment with that setting wrong showed every driver
        /// a permanently empty inbox with nothing to suggest anything was broken.
        /// </remarks>
        Task<List<NotificationDto>?> GetAsync(CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Route signals waiting to be acted on. Never notices: the server keeps the two apart.
        /// </summary>
        /// <remarks>
        /// Signals arrive live over the hub. This drains the ones that arrived while the app
        /// was closed or its socket was down.
        /// </remarks>
        Task<List<NotificationDto>> GetSignalsAsync(CancellationToken cancellationToken = default);

        /// <summary>Deletes a signal the app has already acted on.</summary>
        Task<bool> DeleteSignalAsync(Guid recipientRecordId, CancellationToken cancellationToken = default);

        Task<bool> MarkViewedAsync(Guid recipientRecordId, CancellationToken cancellationToken = default);

        Task<bool> MarkUnviewedAsync(Guid recipientRecordId, CancellationToken cancellationToken = default);

        Task<bool> MarkAcknowledgedAsync(Guid recipientRecordId, CancellationToken cancellationToken = default);

        Task<bool> MarkAllViewedAsync(CancellationToken cancellationToken = default);

        Task<bool> RegisterPushTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Forgets this device on the server. Called on sign out, before the session is wiped.
        /// </summary>
        Task<bool> ClearPushTokenAsync(CancellationToken cancellationToken = default);
    }
}
