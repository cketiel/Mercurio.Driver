namespace Raphael.Driver.DTOs
{
    /// <summary>
    /// ⚠️ Hand copy of <c>Raphael.Notification/Application/DTOs/NotificationRecipientDto.cs</c>.
    /// See <c>_meta/CONTRACT_MAP.md</c>.
    /// </summary>
    public class NotificationRecipientDto
    {
        public Guid Id { get; set; }

        public Guid RecipientId { get; set; }

        public string RecipientType { get; set; } = string.Empty;

        /// <summary>
        /// True when the row addresses a whole audience instead of one person. Always false
        /// for a driver: only the dispatch office is addressed as a group.
        /// </summary>
        public bool IsBroadcast { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? DeliveredAtUtc { get; set; }

        public DateTime? ViewedAtUtc { get; set; }

        public DateTime? AcknowledgedAtUtc { get; set; }
    }
}
