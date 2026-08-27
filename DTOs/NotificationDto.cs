using System.Text.Json.Serialization;

namespace Raphael.Driver.DTOs
{
    /// <summary>
    /// One notification as this application sees it.
    /// </summary>
    /// <remarks>
    /// ⚠️ Hand copy of <c>Raphael.Notification/Application/DTOs/NotificationDto.cs</c>.
    /// Raphael.Shared is not shared with client applications, so this file rots the moment
    /// the backend one changes. See <c>_meta/CONTRACT_MAP.md</c>.
    ///
    /// <para>
    /// <see cref="Title"/> and <see cref="Message"/> are the English text the server rendered;
    /// it is what a push carries. <see cref="Metadata"/> holds the message key, its parameters
    /// and the identifiers the notification is about — identifiers only, never patient data.
    /// </para>
    /// </remarks>
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public string BusinessEventCode { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ExpiresAtUtc { get; set; }

        public List<NotificationRecipientDto> Recipients { get; set; } = new();

        public List<NotificationActionDto> Actions { get; set; } = new();

        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// The row that belongs to this driver. The server only ever returns one.
        /// </summary>
        /// <remarks>
        /// It carries the identifier the view/unview endpoints take, which is the recipient
        /// row and not the notification. Mixing the two silently marks nothing as read.
        /// </remarks>
        [JsonIgnore]
        public NotificationRecipientDto? MyRecipient => Recipients.FirstOrDefault();

        /// <summary>Identifier of the recipient row, for the read/unread endpoints.</summary>
        [JsonIgnore]
        public Guid? RecipientRecordId => MyRecipient?.Id;

        [JsonIgnore]
        public bool IsUnread => MyRecipient?.ViewedAtUtc is null;

        /// <summary>
        /// Trip this notification is about, when there is one. Drives the tap through to the
        /// trip detail; the detail itself is loaded from the API, already authenticated.
        /// </summary>
        [JsonIgnore]
        public int? TripId =>
            Metadata.TryGetValue("TripId", out var raw) && int.TryParse(raw, out var id)
                ? id
                : null;

        [JsonIgnore]
        public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();

        /// <summary>
        /// True when this is a signal for the application rather than a notice for the driver.
        /// </summary>
        /// <remarks>
        /// A signal says the schedule on screen is out of date. The app acts on it and deletes
        /// it; it never belongs in the inbox and never counts on the bell.
        ///
        /// <para>
        /// The server already keeps signals out of the inbox endpoints. This is the second
        /// lock, for the live channel: over the hub everything arrives down the same wire.
        /// </para>
        /// </remarks>
        [JsonIgnore]
        public bool IsSignal =>
            Metadata.TryGetValue("Signal", out var flag) &&
            string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);

        /// <summary>"ADDED" or "REMOVED" on a route signal.</summary>
        [JsonIgnore]
        public string? RouteChange =>
            Metadata.TryGetValue("RouteChange", out var change) ? change : null;

        [JsonIgnore]
        public bool IsTripAddedToRoute => RouteChange == "ADDED";

        [JsonIgnore]
        public bool IsTripRemovedFromRoute => RouteChange == "REMOVED";

        /// <summary>Date of the trip the signal is about, local, when it carries one.</summary>
        [JsonIgnore]
        public DateTime? TripDate =>
            Metadata.TryGetValue("TripDate", out var raw) &&
            DateTime.TryParse(raw, out var parsed)
                ? parsed.Date
                : null;
    }
}
