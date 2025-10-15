using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Entites;


namespace PermiTrack.Controllers;

[ApiController]
[Route("api/users/{userId:long}/roles")]
public class UserRolesController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    public UserRolesController(PermiTrackDbContext db) => _db = db;

    // GET /api/users/{userId}/roles
    [HttpGet]
    public async Task<IActionResult> List(long userId)
    {
        var exists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!exists) return NotFound(new { message = "User not found" });

        var list = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id,
                (ur, r) => new { r.Id, r.Name, r.Level, r.Description, ur.AssignedAt, ur.ExpiresAt, ur.IsActive })
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/users/{userId}/roles/{roleId}
    [HttpPost("{roleId:long}")]
    public async Task<IActionResult> Assign(long userId, long roleId, [FromQuery] long? assignedBy = null, [FromQuery] DateTime? expiresAt = null)
    {
        var user = await _db.Users.FindAsync(userId);
        var role = await _db.Roles.FindAsync(roleId);
        if (user is null || role is null) return NotFound(new { message = "User or Role not found" });

        var exists = await _db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId);
        if (exists) return Conflict(new { message = "Role already assigned to user" });

        _db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy,
            ExpiresAt = expiresAt,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/users/{userId}/roles/{roleId}
    [HttpDelete("{roleId:long}")]
    public async Task<IActionResult> Unassign(long userId, long roleId)
    {
        var link = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
        if (link is null) return NotFound();

        _db.UserRoles.Remove(link);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
