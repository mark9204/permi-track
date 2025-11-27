using PermiTrack.DataContext.Enums;
using System;

namespace PermiTrack.DataContext.DTOs;

/// <summary>
/// Request DTO for creating a notification
/// </summary>
public class CreateNotificationDTO
{
    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Type of related resource (optional)
    /// </summary>
    public string? RelatedResourceType { get; set; }

    /// <summary>
    /// ID of related resource (optional)
    /// </summary>
    public long? RelatedResourceId { get; set; }

    /// <summary>
    /// When the notification should expire (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
