using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Enums;

namespace PermiTrack.Services.Interfaces;

/// <summary>
/// Service for managing user notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a user
    /// </summary>
    /// <param name="userId">User ID to send notification to</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="type">Type of notification</param>
    /// <param name="relatedResourceType">Type of related resource (optional)</param>
    /// <param name="relatedResourceId">ID of related resource (optional)</param>
    /// <param name="expiresAt">When notification expires (optional)</param>
    /// <returns>Created notification</returns>
    Task<NotificationDTO> SendNotificationAsync(
        long userId,
        string title,
        string message,
        NotificationType type,
        string? relatedResourceType = null,
        long? relatedResourceId = null,
        DateTime? expiresAt = null);

    /// <summary>
    /// Send a notification using DTO
    /// </summary>
    /// <param name="userId">User ID to send notification to</param>
    /// <param name="notificationDto">Notification details</param>
    /// <returns>Created notification</returns>
    Task<NotificationDTO> SendNotificationAsync(long userId, CreateNotificationDTO notificationDto);

    /// <summary>
    /// Get notifications for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="unreadOnly">If true, only return unread notifications</param>
    /// <param name="pageSize">Maximum number of notifications to return</param>
    /// <returns>List of notifications</returns>
    Task<IEnumerable<NotificationDTO>> GetMyNotificationsAsync(long userId, bool unreadOnly = false, int pageSize = 50);

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Updated notification</returns>
    Task<NotificationDTO> MarkAsReadAsync(long notificationId, long userId);

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of notifications marked as read</returns>
    Task<int> MarkAllAsReadAsync(long userId);

    /// <summary>
    /// Get count of unread notifications for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Count of unread notifications</returns>
    Task<int> GetUnreadCountAsync(long userId);

    /// <summary>
    /// Delete a notification
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteNotificationAsync(long notificationId, long userId);

    /// <summary>
    /// Delete all read notifications for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of notifications deleted</returns>
    Task<int> DeleteReadNotificationsAsync(long userId);

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Notification details or null if not found</returns>
    Task<NotificationDTO?> GetNotificationByIdAsync(long notificationId, long userId);
}
