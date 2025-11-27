using System;

namespace PermiTrack.DataContext.Entites;

/// <summary>
/// Entity for logging HTTP requests and responses for audit purposes.
/// This is used by the AuditLoggingMiddleware to track all API calls.
/// </summary>
public class HttpAuditLog
{
    public long Id { get; set; }

    /// <summary>
    /// User ID if authenticated, null for anonymous requests
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// HTTP Method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Request path (e.g., /api/users)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Query string if present (e.g., ?page=1&size=10)
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// HTTP Status Code (200, 401, 403, 404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Client IP Address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent string from request header
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Request duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Timestamp when the request was made
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username if authenticated, null for anonymous
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Additional context or error information
    /// </summary>
    public string? AdditionalInfo { get; set; }

    // Navigation property
    public User? User { get; set; }
}
