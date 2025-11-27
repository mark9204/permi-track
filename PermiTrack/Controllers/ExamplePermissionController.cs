using Microsoft.AspNetCore.Mvc;
using PermiTrack.Authorization;

namespace PermiTrack.Controllers;

/// <summary>
/// Example controller demonstrating permission-based authorization.
/// This serves as a reference for how to use the RequirePermission attribute.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExamplePermissionController : ControllerBase
{
    /// <summary>
    /// Example endpoint requiring "Users.Read" permission.
    /// Users must have this permission in their JWT claims to access this endpoint.
    /// </summary>
    [HttpGet("users")]
    [RequirePermission("Users.Read")]
    public IActionResult GetUsers()
    {
        return Ok(new
        {
            message = "You have permission to read users!",
            data = new[] { "User1", "User2", "User3" }
        });
    }

    /// <summary>
    /// Example endpoint requiring "Users.Write" permission.
    /// </summary>
    [HttpPost("users")]
    [RequirePermission("Users.Write")]
    public IActionResult CreateUser([FromBody] object userData)
    {
        return Ok(new
        {
            message = "You have permission to create users!",
            created = true
        });
    }

    /// <summary>
    /// Example endpoint requiring "Roles.Manage" permission.
    /// </summary>
    [HttpPut("roles/{roleId}")]
    [RequirePermission("Roles.Manage")]
    public IActionResult UpdateRole(int roleId, [FromBody] object roleData)
    {
        return Ok(new
        {
            message = "You have permission to manage roles!",
            roleId = roleId,
            updated = true
        });
    }

    /// <summary>
    /// Example endpoint requiring "Permissions.Admin" permission.
    /// This demonstrates a high-level administrative permission.
    /// </summary>
    [HttpDelete("permissions/{permissionId}")]
    [RequirePermission("Permissions.Admin")]
    public IActionResult DeletePermission(int permissionId)
    {
        return Ok(new
        {
            message = "You have admin permission to delete permissions!",
            permissionId = permissionId,
            deleted = true
        });
    }

    /// <summary>
    /// Public endpoint - no permission required.
    /// </summary>
    [HttpGet("public")]
    public IActionResult GetPublicData()
    {
        return Ok(new
        {
            message = "This is public data - no permission required",
            data = "Public information"
        });
    }
}
