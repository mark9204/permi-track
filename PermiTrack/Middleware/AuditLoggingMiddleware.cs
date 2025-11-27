using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext.Entites;
using PermiTrack.Services.Interfaces;
using System.Diagnostics;
using System.Security.Claims;

namespace PermiTrack.Middleware;

/// <summary>
/// Middleware for logging all HTTP requests and responses to the audit log.
/// This middleware captures request details, user information, and response metrics.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Start timing the request
        var stopwatch = Stopwatch.StartNew();
        
        // Store original response body stream
        var originalBodyStream = context.Response.Body;
        
        try
        {
            // Execute the next middleware in the pipeline
            await _next(context);
        }
        finally
        {
            // Stop timing
            stopwatch.Stop();

            // Log the request asynchronously - don't block the response
            // Use Task.Run to avoid blocking and to ensure the audit log is saved
            // even if the client disconnects
            _ = Task.Run(async () =>
            {
                try
                {
                    var auditLog = CreateAuditLog(context, stopwatch.ElapsedMilliseconds);
                    await auditService.LogSafeAsync(auditLog);
                }
                catch (Exception ex)
                {
                    // Last resort error handling - log but don't throw
                    _logger.LogError(ex, "Critical error in audit logging middleware");
                }
            });
        }
    }

    private HttpAuditLog CreateAuditLog(HttpContext context, long durationMs)
    {
        var request = context.Request;
        var user = context.User;

        // Extract user information
        long? userId = null;
        string? username = null;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try to get user ID from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            // Get username
            username = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity.Name;
        }

        // Get IP Address - handle different scenarios
        var ipAddress = GetClientIpAddress(context);

        // Get User Agent
        var userAgent = request.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent))
        {
            userAgent = "Unknown";
        }

        // Build query string
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : null;

        // Create audit log entry
        return new HttpAuditLog
        {
            UserId = userId,
            Username = username,
            Method = request.Method,
            Path = request.Path,
            QueryString = queryString,
            StatusCode = context.Response.StatusCode,
            IpAddress = ipAddress,
            UserAgent = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
            DurationMs = durationMs,
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (for proxied requests)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to direct connection IP
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(remoteIp))
        {
            // Handle IPv6 localhost
            if (remoteIp == "::1")
            {
                return "127.0.0.1";
            }
            return remoteIp;
        }

        return "Unknown";
    }
}
