using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/assign")]
public class RoleGrantsController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    public RoleGrantsController(PermiTrackDbContext db) => _db = db;

    // POST /api/assign/role
    [HttpPost("role")]
    public async Task<IActionResult> GrantRole([FromBody] GrantRoleRequest req)
    {
        // 1) target user + role felkutatása
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null) return NotFound(new { message = $"User '{req.Username}' not found" });

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == req.RoleName);
        if (role is null) return NotFound(new { message = $"Role '{req.RoleName}' not found" });

        // 2) dup check
        var exists = await _db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (exists) return Conflict(new { message = "User already has this role" });

        // 3) hozzárendelés
        var link = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = req.AdminUserId,       // amíg nincs JWT
            ExpiresAt = req.ExpiresAt,
            IsActive = true
        };
        _db.UserRoles.Add(link);

        // 4) Audit log
        var admin = await _db.Users.FindAsync(req.AdminUserId); // lehet null (system)
        var newValues = new
        {
            link.UserId,
            link.RoleId,
            link.AssignedAt,
            link.AssignedBy,
            link.ExpiresAt,
            link.IsActive,
            Reason = req.Reason
        };

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = req.AdminUserId == 0 ? null : req.AdminUserId,   // null = system
            Action = "ASSIGN_ROLE",
            ResourceType = "UserRoles",
            ResourceId = 0, // nincs még Id, de később frissíthetnénk SaveChanges után
            OldValues = null,
            NewValues = JsonSerializer.Serialize(newValues),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(); // itt kap Id-t a link

        return Ok(new
        {
            message = $"Role '{role.Name}' assigned to user '{user.Username}'",
            userId = user.Id,
            roleId = role.Id,
            userRoleId = link.Id,
            expiresAt = link.ExpiresAt
        });
    }
}
