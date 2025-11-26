using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using System.Data;


namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class RolesController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    private readonly IMapper _map;
    public RolesController(PermiTrackDbContext db, IMapper map) { _db = db; _map = map; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDTO>>> GetAll() =>
        Ok(await _db.Roles.AsNoTracking().ProjectTo<RoleDTO>(_map.ConfigurationProvider).ToListAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<RoleDTO>> Get(long id)
    {
        var dto = await _db.Roles.AsNoTracking().Where(r => r.Id == id)
            .ProjectTo<RoleDTO>(_map.ConfigurationProvider).FirstOrDefaultAsync();
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest req)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == req.Name))
            return Conflict(new { message = "Role already exists" });

        var role = new Role
        {
            Name = req.Name,
            Description = req.Description,
            IsActive = req.IsActive,
            Level = req.Level,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = role.Id }, role);
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<RoleDTO>> Update(long id, RoleDTO dto)
    {
        var e = await _db.Roles.FindAsync(id);
        if (e is null) return NotFound();
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.IsActive = dto.IsActive;
        e.Level = dto.Level;
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(_map.Map<RoleDTO>(e));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var e = await _db.Roles.FindAsync(id);
        if (e is null) return NotFound();
        _db.Roles.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
