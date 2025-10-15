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
public class UsersController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    private readonly IMapper _map;
    public UsersController(PermiTrackDbContext db, IMapper map)
    {
        _db = db; _map = map;
    }

    // GET /api/users?skip=0&take=50&search=joe
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromQuery] string? search = null)
    {
        var q = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

        var list = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip(Math.Max(0, skip))
            .Take(Math.Clamp(take, 1, 200))
            .ProjectTo<UserDTO>(_map.ConfigurationProvider)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDTO>> GetOne(long id)
    {
        var dto = await _db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<UserDTO>(_map.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<UserDTO>> Create([FromBody] CreateUserRequest req) // Első dto
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { field = "Username", message = "already exists" });
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { field = "Email", message = "already exists" });

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = req.PasswordHash,   // TODO: később hash-elni
            FirstName = req.FirstName,
            LastName = req.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var dto = _map.Map<UserDTO>(user);
        return CreatedAtAction(nameof(GetOne), new { id = user.Id }, dto);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserDTO>> Update(long id, [FromBody] UpdateUserRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (req.Email != null) user.Email = req.Email;
        if (req.FirstName != null) user.FirstName = req.FirstName;
        if (req.LastName != null) user.LastName = req.LastName;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(_map.Map<UserDTO>(user));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
