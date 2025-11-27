using PermiTrack.DataContext.Enums;
using System;

namespace PermiTrack.DataContext.DTOs;

/// <summary>
/// Response DTO for notification details
/// </summary>
public class NotificationDTO
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? RelatedResourceType { get; set; }
    public long? RelatedResourceId { get; set; }
}
