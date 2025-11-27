using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.Authorization;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Entites;

namespace PermiTrack.Controllers;

/// <summary>
/// Controller for querying HTTP audit logs from the audit logging middleware.
/// Allows administrators to view and analyze API access patterns and security events.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[RequirePermission("AuditLogs.Read")]
public class HttpAuditLogsController : ControllerBase
{
    private readonly PermiTrackDbContext _context;
    private readonly ILogger<HttpAuditLogsController> _logger;

    public HttpAuditLogsController(
        PermiTrackDbContext context,
        ILogger<HttpAuditLogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated HTTP audit logs with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHttpAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? username = null,
        [FromQuery] string? method = null,
        [FromQuery] string? path = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 1000) pageSize = 1000; // Limit max page size

            var query = _context.HttpAuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(a => a.Username != null && a.Username.Contains(username));
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                query = query.Where(a => a.Method == method.ToUpper());
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                query = query.Where(a => a.Path.Contains(path));
            }

            if (statusCode.HasValue)
            {
                query = query.Where(a => a.StatusCode == statusCode.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.Username,
                    a.Method,
                    a.Path,
                    a.QueryString,
                    a.StatusCode,
                    a.IpAddress,
                    a.UserAgent,
                    a.DurationMs,
                    a.Timestamp,
                    a.AdditionalInfo
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTTP audit logs");
            return StatusCode(500, new { message = "Error retrieving HTTP audit logs" });
        }
    }

    /// <summary>
    /// Get a specific HTTP audit log entry by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHttpAuditLogById(long id)
    {
        try
        {
            var log = await _context.HttpAuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (log == null)
            {
                return NotFound(new { message = "HTTP audit log not found" });
            }

            return Ok(new
            {
                log.Id,
                log.UserId,
                log.Username,
                User = log.User != null ? new
                {
                    log.User.Id,
                    log.User.Username,
                    log.User.Email,
                    log.User.FirstName,
                    log.User.LastName
                } : null,
                log.Method,
                log.Path,
                log.QueryString,
                log.StatusCode,
                log.IpAddress,
                log.UserAgent,
                log.DurationMs,
                log.Timestamp,
                log.AdditionalInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTTP audit log {Id}", id);
            return StatusCode(500, new { message = "Error retrieving HTTP audit log" });
        }
    }

    /// <summary>
    /// Get HTTP audit log statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.HttpAuditLogs.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            var totalRequests = await query.CountAsync();
            var uniqueUsers = await query.Where(a => a.UserId != null).Select(a => a.UserId).Distinct().CountAsync();
            var averageDuration = await query.AverageAsync(a => (double?)a.DurationMs) ?? 0;

            var statusCodeDistribution = await query
                .GroupBy(a => a.StatusCode)
                .Select(g => new { StatusCode = g.Key, Count = g.Count() })
                .OrderBy(x => x.StatusCode)
                .ToListAsync();

            var methodDistribution = await query
                .GroupBy(a => a.Method)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topPaths = await query
                .GroupBy(a => a.Path)
                .Select(g => new { Path = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalRequests,
                uniqueUsers,
                averageDurationMs = Math.Round(averageDuration, 2),
                statusCodeDistribution,
                methodDistribution,
                topPaths
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTTP audit log statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    /// <summary>
    /// Get HTTP audit logs for a specific user.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserHttpAuditLogs(
        long userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 1000) pageSize = 1000;

            var query = _context.HttpAuditLogs.Where(a => a.UserId == userId);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.Method,
                    a.Path,
                    a.QueryString,
                    a.StatusCode,
                    a.IpAddress,
                    a.DurationMs,
                    a.Timestamp
                })
                .ToListAsync();

            return Ok(new
            {
                userId,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTTP audit logs for user {UserId}", userId);
            return StatusCode(500, new { message = "Error retrieving user HTTP audit logs" });
        }
    }

    /// <summary>
    /// Get failed authentication attempts (401/403 status codes).
    /// </summary>
    [HttpGet("security/failed-attempts")]
    public async Task<IActionResult> GetFailedAttempts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? fromDate = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 1000) pageSize = 1000;

            var query = _context.HttpAuditLogs
                .Where(a => a.StatusCode == 401 || a.StatusCode == 403);

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.Username,
                    a.Method,
                    a.Path,
                    a.StatusCode,
                    a.IpAddress,
                    a.Timestamp
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed authentication attempts");
            return StatusCode(500, new { message = "Error retrieving failed attempts" });
        }
    }
}
