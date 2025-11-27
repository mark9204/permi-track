using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Entites;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

/// <summary>
/// Service for logging HTTP requests and responses for audit purposes.
/// Uses a separate DbContext instance to avoid conflicts with the request's DbContext.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IDbContextFactory<PermiTrackDbContext> _contextFactory;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IDbContextFactory<PermiTrackDbContext> contextFactory,
        ILogger<AuditService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Logs an HTTP request/response to the database.
    /// </summary>
    public async Task LogAsync(HttpAuditLog auditLog)
    {
        // Create a new DbContext instance for this operation
        // This prevents conflicts with the request's DbContext
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        context.HttpAuditLogs.Add(auditLog);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Logs an HTTP request/response with error handling.
    /// This method will not throw exceptions to prevent disrupting the request pipeline.
    /// </summary>
    public async Task LogSafeAsync(HttpAuditLog auditLog)
    {
        try
        {
            await LogAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want audit logging to break the API
            _logger.LogError(ex, 
                "Failed to save audit log for {Method} {Path}. StatusCode: {StatusCode}", 
                auditLog.Method, 
                auditLog.Path, 
                auditLog.StatusCode);
        }
    }
}
