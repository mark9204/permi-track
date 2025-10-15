using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Entites;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/roles/{roleId:long}/permissions")]
public class RolePermissionsController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    public RolePermissionsController(PermiTrackDbContext db) => _db = db;

    // GET /api/roles/{roleId}/permissions
    [HttpGet]
    public async Task<IActionResult> List(long roleId)
    {
        var exists = await _db.Roles.AnyAsync(r => r.Id == roleId);
        if (!exists) return NotFound(new { message = "Role not found" });

        var list = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id,
                (rp, p) => new { p.Id, p.Name, p.Resource, p.Action, p.Description, rp.GrantedAt, rp.GrantedBy })
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/roles/{roleId}/permissions/{permissionId}
    [HttpPost("{permissionId:long}")]
    public async Task<IActionResult> Assign(long roleId, long permissionId, [FromQuery] long? grantedBy = null)
    {
        var role = await _db.Roles.FindAsync(roleId);
        var perm = await _db.Permissions.FindAsync(permissionId);
        if (role is null || perm is null) return NotFound(new { message = "Role or Permission not found" });

        var exists = await _db.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
        if (exists) return Conflict(new { message = "Permission already assigned to role" });

        _db.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = grantedBy
        });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/roles/{roleId}/permissions/{permissionId}
    [HttpDelete("{permissionId:long}")]
    public async Task<IActionResult> Unassign(long roleId, long permissionId)
    {
        var link = await _db.RolePermissions
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
        if (link is null) return NotFound();

        _db.RolePermissions.Remove(link);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
