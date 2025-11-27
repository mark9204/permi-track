using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using System.Security.Claims;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly PermiTrackDbContext _db;

    public SessionController(PermiTrackDbContext db)
    {
        _db = db;
    }

    // Get all active sessions for the current user
    [HttpGet("my-sessions")]
    public async Task<ActionResult<IEnumerable<SessionDTO>>> GetMySessions()
    {
        var userId = GetCurrentUserId();
        
        var sessions = await _db.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .Select(s => new SessionDTO
            {
                Id = s.Id,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                CreatedAt = s.CreatedAt,
                LastActivity = s.LastActivity,
                ExpiresAt = s.ExpiresAt
            })
            .ToListAsync();

        return Ok(sessions);
    }

    // Terminate a specific session
    [HttpDelete("{sessionId:long}")]
    public async Task<IActionResult> TerminateSession(long sessionId)
    {
        var userId = GetCurrentUserId();
        
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        session.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Terminate all sessions except the current one
    [HttpPost("terminate-all-others")]
    public async Task<IActionResult> TerminateAllOtherSessions()
    {
        var userId = GetCurrentUserId();
        
        // Get the current session token from the request header
        var currentToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        // Find all active sessions except the current one
        var sessions = await _db.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            // Keep the current session active
            if (session.TokenHash != currentToken)
            {
                session.IsActive = false;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "All other sessions terminated successfully" });
    }

    // Terminate all sessions (logout from all devices)
    [HttpPost("terminate-all")]
    public async Task<IActionResult> TerminateAllSessions()
    {
        var userId = GetCurrentUserId();
        
        var sessions = await _db.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "All sessions terminated successfully" });
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
