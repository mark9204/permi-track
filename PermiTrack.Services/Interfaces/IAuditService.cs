using PermiTrack.DataContext.Entites;

namespace PermiTrack.Services.Interfaces;

/// <summary>
/// Service for logging HTTP requests and responses for audit purposes.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an HTTP request/response to the database.
    /// </summary>
    /// <param name="auditLog">The audit log entry to save</param>
    /// <returns>Task representing the async operation</returns>
    Task LogAsync(HttpAuditLog auditLog);

    /// <summary>
    /// Logs an HTTP request/response to the database with error handling.
    /// This method will not throw exceptions to prevent disrupting the request pipeline.
    /// </summary>
    /// <param name="auditLog">The audit log entry to save</param>
    /// <returns>Task representing the async operation</returns>
    Task LogSafeAsync(HttpAuditLog auditLog);
}
