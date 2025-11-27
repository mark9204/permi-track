using PermiTrack.DataContext.Entites;

namespace PermiTrack.Services.Interfaces;

/// <summary>
/// Service for security-related operations
/// SPEC 7: Security - Login Tracking and Monitoring
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Record a login attempt for security monitoring
    /// </summary>
    /// <param name="userName">Username entered in login form</param>
    /// <param name="ipAddress">IP address of the attempt</param>
    /// <param name="userAgent">User agent (browser/client info)</param>
    /// <param name="isSuccess">Whether login was successful</param>
    /// <param name="userId">User ID if login successful or user exists</param>
    /// <param name="failureReason">Reason for failure (e.g., "InvalidPassword", "AccountLocked")</param>
    /// <returns>Created login attempt record</returns>
    Task<LoginAttempt> RecordLoginAttemptAsync(
        string userName,
        string ipAddress,
        string userAgent,
        bool isSuccess,
        long? userId = null,
        string? failureReason = null);

    /// <summary>
    /// Get recent failed login attempts for a user (for security analysis)
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>List of failed login attempts</returns>
    Task<IEnumerable<LoginAttempt>> GetRecentFailedAttemptsAsync(string userName, int hours = 24);

    /// <summary>
    /// Get failed login attempts from a specific IP address
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>List of failed login attempts</returns>
    Task<IEnumerable<LoginAttempt>> GetFailedAttemptsByIpAsync(string ipAddress, int hours = 24);

    /// <summary>
    /// Get all login attempts for a user (for user activity history)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageSize">Number of records to return</param>
    /// <returns>List of login attempts</returns>
    Task<IEnumerable<LoginAttempt>> GetUserLoginHistoryAsync(long userId, int pageSize = 50);
}
