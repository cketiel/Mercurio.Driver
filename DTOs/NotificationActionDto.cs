namespace Raphael.Driver.DTOs
{
    /// <summary>
    /// ⚠️ Hand copy of <c>Raphael.Notification/Application/DTOs/NotificationActionDto.cs</c>.
    /// See <c>_meta/CONTRACT_MAP.md</c>.
    /// </summary>
    public class NotificationActionDto
    {
        public Guid Id { get; set; }

        public string ActionCode { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsPrimary { get; set; }
    }
}
