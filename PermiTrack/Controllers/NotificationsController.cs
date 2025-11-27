using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermiTrack.DataContext.DTOs;
using PermiTrack.Services.Interfaces;
using System.Security.Claims;

namespace PermiTrack.Controllers;

/// <summary>
/// Controller for managing user notifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's notifications
    /// </summary>
    /// <param name="unreadOnly">If true, only return unread notifications</param>
    /// <param name="pageSize">Number of notifications to return (max 100)</param>
    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId, unreadOnly, pageSize);

            return Ok(new
            {
                userId,
                unreadOnly,
                count = notifications.Count(),
                notifications
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return StatusCode(500, new { message = "An error occurred while retrieving notifications" });
        }
    }

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotificationById(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id, userId);

            if (notification == null)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the notification" });
        }
    }

    /// <summary>
    /// Get unread notification count for current user
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new { userId, unreadCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread count");
            return StatusCode(500, new { message = "An error occurred while retrieving unread count" });
        }
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.MarkAsReadAsync(id, userId);

            return Ok(new
            {
                message = "Notification marked as read",
                notification
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to mark notification {NotificationId} as read", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, new { message = "An error occurred while marking the notification as read" });
        }
    }

    /// <summary>
    /// Mark all notifications as read for current user
    /// </summary>
    [HttpPut("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new
            {
                message = $"Marked {count} notifications as read",
                markedCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new { message = "An error occurred while marking notifications as read" });
        }
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.DeleteNotificationAsync(id, userId);

            if (!success)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return Ok(new { message = "Notification deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the notification" });
        }
    }

    /// <summary>
    /// Delete all read notifications for current user
    /// </summary>
    [HttpDelete("clear-read")]
    public async Task<IActionResult> DeleteReadNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.DeleteReadNotificationsAsync(userId);

            return Ok(new
            {
                message = $"Deleted {count} read notifications",
                deletedCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting read notifications");
            return StatusCode(500, new { message = "An error occurred while deleting notifications" });
        }
    }

    /// <summary>
    /// Send a notification to a specific user (admin only)
    /// </summary>
    [HttpPost("send")]
    [Authorization.RequirePermission("Notifications.Send")]
    public async Task<IActionResult> SendNotification(
        [FromQuery] long userId,
        [FromBody] CreateNotificationDTO notificationDto)
    {
        try
        {
            var notification = await _notificationService.SendNotificationAsync(userId, notificationDto);

            return CreatedAtAction(
                nameof(GetNotificationById),
                new { id = notification.Id },
                new
                {
                    message = "Notification sent successfully",
                    notification
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while sending the notification" });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
