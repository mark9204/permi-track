using System;

namespace PermiTrack.DataContext.Entites;

/// <summary>
/// Entity for tracking login attempts for security monitoring
/// SPEC 7: Security - Login Tracking
/// </summary>
public class LoginAttempt
{
    public long Id { get; set; }

    /// <summary>
    /// User ID if user exists (null for non-existent users)
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Username that was entered in the login form
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// IP Address of the login attempt
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent (browser/client information)
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Whether the login attempt was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Reason for failure (e.g., "InvalidPassword", "AccountLocked", "UserNotFound")
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// When the login attempt occurred
    /// </summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
