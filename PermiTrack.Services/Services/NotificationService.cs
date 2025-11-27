using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using PermiTrack.DataContext.Enums;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

/// <summary>
/// Service for managing user notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly PermiTrackDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        PermiTrackDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationDTO> SendNotificationAsync(
        long userId,
        string title,
        string message,
        NotificationType type,
        string? relatedResourceType = null,
        long? relatedResourceId = null,
        DateTime? expiresAt = null)
    {
        // Validate user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Create notification
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedResourceType = relatedResourceType,
            RelatedResourceId = relatedResourceId,
            ExpiresAt = expiresAt
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification sent to user {UserId}: {Title} (Type: {Type})",
            userId, title, type);

        return MapToDTO(notification);
    }

    public async Task<NotificationDTO> SendNotificationAsync(long userId, CreateNotificationDTO notificationDto)
    {
        return await SendNotificationAsync(
            userId,
            notificationDto.Title,
            notificationDto.Message,
            notificationDto.Type,
            notificationDto.RelatedResourceType,
            notificationDto.RelatedResourceId,
            notificationDto.ExpiresAt);
    }

    public async Task<IEnumerable<NotificationDTO>> GetMyNotificationsAsync(
        long userId,
        bool unreadOnly = false,
        int pageSize = 50)
    {
        // Validate page size
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        // Filter for unread only if requested
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        // Order by creation date (newest first) and apply pagination
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        return notifications.Select(MapToDTO);
    }

    public async Task<NotificationDTO> MarkAsReadAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new InvalidOperationException("Notification not found or does not belong to this user");
        }

        // Only update if not already read
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notification {NotificationId} marked as read by user {UserId}",
                notificationId, userId);
        }

        return MapToDTO(notification);
    }

    public async Task<int> MarkAllAsReadAsync(long userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Marked {Count} notifications as read for user {UserId}",
            unreadNotifications.Count, userId);

        return unreadNotifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> DeleteNotificationAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return false;
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification {NotificationId} deleted by user {UserId}",
            notificationId, userId);

        return true;
    }

    public async Task<int> DeleteReadNotificationsAsync(long userId)
    {
        var readNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead)
            .ToListAsync();

        if (readNotifications.Count == 0)
        {
            return 0;
        }

        _context.Notifications.RemoveRange(readNotifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted {Count} read notifications for user {UserId}",
            readNotifications.Count, userId);

        return readNotifications.Count;
    }

    public async Task<NotificationDTO?> GetNotificationByIdAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        return notification != null ? MapToDTO(notification) : null;
    }

    private NotificationDTO MapToDTO(Notification notification)
    {
        return new NotificationDTO
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            ExpiresAt = notification.ExpiresAt,
            RelatedResourceType = notification.RelatedResourceType,
            RelatedResourceId = notification.RelatedResourceId
        };
    }
}
