using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;


namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    private readonly IMapper _map;
    public PermissionsController(PermiTrackDbContext db, IMapper map) { _db = db; _map = map; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDTO>>> GetAll() =>
        Ok(await _db.Permissions.AsNoTracking().ProjectTo<PermissionDTO>(_map.ConfigurationProvider).ToListAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PermissionDTO>> Get(long id)
    {
        var dto = await _db.Permissions.AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectTo<PermissionDTO>(_map.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<PermissionDTO>> Create(PermissionDTO dto)
    {
        if (await _db.Permissions.AnyAsync(p => p.Name == dto.Name)) return Conflict(new { message = "Permission exists" });
        var e = _map.Map<Permission>(dto);
        e.CreatedAt = DateTime.UtcNow;
        _db.Permissions.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.Id }, _map.Map<PermissionDTO>(e));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<PermissionDTO>> Update(long id, PermissionDTO dto)
    {
        var e = await _db.Permissions.FindAsync(id);
        if (e is null) return NotFound();
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.Resource = dto.Resource;
        e.Action = dto.Action;
        e.IsActive = dto.IsActive;
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(_map.Map<PermissionDTO>(e));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var e = await _db.Permissions.FindAsync(id);
        if (e is null) return NotFound();
        _db.Permissions.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
