using PermiTrack.Middleware;

namespace PermiTrack.Extensions;

/// <summary>
/// Extension methods for registering the Audit Logging Middleware.
/// </summary>
public static class AuditLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds the Audit Logging Middleware to the application pipeline.
    /// This should be placed after Authentication and Authorization middleware
    /// but before endpoint mapping to ensure user context is available.
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLoggingMiddleware>();
    }
}
