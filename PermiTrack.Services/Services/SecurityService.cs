using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Entites;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

/// <summary>
/// Service for security-related operations
/// SPEC 7: Security - Login Tracking and Monitoring
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly PermiTrackDbContext _context;
    private readonly ILogger<SecurityService> _logger;

    public SecurityService(
        PermiTrackDbContext context,
        ILogger<SecurityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// SPEC 7: Record a login attempt for security monitoring and analysis
    /// </summary>
    public async Task<LoginAttempt> RecordLoginAttemptAsync(
        string userName,
        string ipAddress,
        string userAgent,
        bool isSuccess,
        long? userId = null,
        string? failureReason = null)
    {
        var loginAttempt = new LoginAttempt
        {
            UserId = userId,
            UserName = userName,
            IpAddress = ipAddress,
            UserAgent = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            AttemptedAt = DateTime.UtcNow
        };

        _context.LoginAttempts.Add(loginAttempt);
        await _context.SaveChangesAsync();

        // Log security event
        if (isSuccess)
        {
            _logger.LogInformation(
                "Successful login: User={UserName}, UserId={UserId}, IP={IpAddress}",
                userName, userId, ipAddress);
        }
        else
        {
            _logger.LogWarning(
                "Failed login attempt: User={UserName}, Reason={Reason}, IP={IpAddress}",
                userName, failureReason, ipAddress);
        }

        return loginAttempt;
    }

    /// <summary>
    /// SPEC 7: Get recent failed login attempts for security analysis
    /// </summary>
    public async Task<IEnumerable<LoginAttempt>> GetRecentFailedAttemptsAsync(string userName, int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);

        return await _context.LoginAttempts
            .Where(la => la.UserName == userName &&
                        !la.IsSuccess &&
                        la.AttemptedAt >= cutoffTime)
            .OrderByDescending(la => la.AttemptedAt)
            .ToListAsync();
    }

    /// <summary>
    /// SPEC 7: Get failed login attempts from a specific IP for brute force detection
    /// </summary>
    public async Task<IEnumerable<LoginAttempt>> GetFailedAttemptsByIpAsync(string ipAddress, int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);

        return await _context.LoginAttempts
            .Where(la => la.IpAddress == ipAddress &&
                        !la.IsSuccess &&
                        la.AttemptedAt >= cutoffTime)
            .OrderByDescending(la => la.AttemptedAt)
            .ToListAsync();
    }

    /// <summary>
    /// SPEC 7: Get user's login history for activity monitoring
    /// </summary>
    public async Task<IEnumerable<LoginAttempt>> GetUserLoginHistoryAsync(long userId, int pageSize = 50)
    {
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 100) pageSize = 100;

        return await _context.LoginAttempts
            .Where(la => la.UserId == userId)
            .OrderByDescending(la => la.AttemptedAt)
            .Take(pageSize)
            .ToListAsync();
    }
}
