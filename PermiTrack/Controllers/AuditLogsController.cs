using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditLogsController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    public AuditLogsController(PermiTrackDbContext db) => _db = db;

    // GET /api/audit?take=50
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int take = 50)
    {
        take = Math.Clamp(take, 1, 200);
        var items = await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new {
                x.Id,
                x.CreatedAt,
                x.UserId,
                x.Action,
                x.ResourceType,
                x.ResourceId,
                x.IpAddress,
                x.UserAgent,
                x.NewValues,
                x.OldValues
            })
            .ToListAsync();

        return Ok(items);
    }
}
