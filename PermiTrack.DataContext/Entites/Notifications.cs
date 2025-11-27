using PermiTrack.DataContext.Enums;
using System;

namespace PermiTrack.DataContext.Entites
{
    /// <summary>
    /// Represents a notification for a user
    /// </summary>
    public class Notification
    {
        public long Id { get; set; }

        /// <summary>
        /// User who should receive this notification
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Notification title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notification message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type of notification (Info, Success, Warning, Error)
        /// </summary>
        public NotificationType Type { get; set; } = NotificationType.Info;

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When the notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the notification was read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// When the notification should expire and be auto-deleted
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Type of the related resource (e.g., "AccessRequest", "UserRole")
        /// </summary>
        public string? RelatedResourceType { get; set; }

        /// <summary>
        /// ID of the related resource
        /// </summary>
        public long? RelatedResourceId { get; set; }

        // Navigation property
        public User User { get; set; } = default!;
    }
}
