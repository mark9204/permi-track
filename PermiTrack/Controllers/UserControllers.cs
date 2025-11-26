using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermiTrack.DataContext.DTOs;
using PermiTrack.Services.Interfaces;
using System.Security.Claims;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    // GET /api/users - List all users with pagination and filters
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (success, message, users, totalCount) = await _userManagementService.GetUsersAsync(
            page, pageSize, search, isActive);

        if (!success)
            return BadRequest(new { message });

        return Ok(new
        {
            data = users,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    // GET /api/users/{id} - Get user details
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var (success, message, user) = await _userManagementService.GetUserByIdAsync(id);

        if (!success)
            return NotFound(new { message });

        return Ok(user);
    }

    // POST /api/users - Create new user (admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var (success, message, userId) = await _userManagementService.CreateUserAsync(request, adminUserId);

        if (!success)
            return BadRequest(new { message });

        return CreatedAtAction(nameof(GetUser), new { id = userId }, new { id = userId, message });
    }

    // PUT /api/users/{id} - Update user (admin only)
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var (success, message) = await _userManagementService.UpdateUserAsync(id, request, adminUserId);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    // DELETE /api/users/{id} - Soft delete user (admin only)
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(long id, [FromQuery] string? reason = null)
    {
        var adminUserId = GetCurrentUserId();

        // Prevent self-deletion
        if (id == adminUserId)
            return BadRequest(new { message = "You cannot delete your own account" });

        var (success, message) = await _userManagementService.DeleteUserAsync(id, adminUserId, reason);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    // POST /api/users/{id}/activate - Activate user (admin only)
    [HttpPost("{id:long}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(long id, [FromBody] UserActivationRequest? request = null)
    {
        var adminUserId = GetCurrentUserId();
        var (success, message) = await _userManagementService.SetUserActiveStatusAsync(
            id, true, adminUserId, request?.Reason);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    // POST /api/users/{id}/deactivate - Deactivate user (admin only)
    [HttpPost("{id:long}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(long id, [FromBody] UserActivationRequest request)
    {
        var adminUserId = GetCurrentUserId();

        // Prevent self-deactivation
        if (id == adminUserId)
            return BadRequest(new { message = "You cannot deactivate your own account" });

        var (success, message) = await _userManagementService.SetUserActiveStatusAsync(
            id, false, adminUserId, request.Reason);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    // POST /api/users/bulk-import - Import multiple users from CSV/JSON (admin only)
    [HttpPost("bulk-import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkImport([FromBody] BulkUserImportRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _userManagementService.BulkImportUsersAsync(request, adminUserId);

        return Ok(result);
    }
}
