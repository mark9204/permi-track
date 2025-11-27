using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.Services.Interfaces;
using System.Security.Claims;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public AccountController(PermiTrackDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // Get current user's profile
    [HttpGet("profile")]
    public async Task<ActionResult<UserDTO>> GetProfile()
    {
        var userId = GetCurrentUserId();
        
        var user = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    // Change current user's password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Current password is incorrect" });
        }

        // Update to new password
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Optionally invalidate all other sessions for security
        var sessions = await _db.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully. Please login again." });
    }

    // Update current user's profile
    [HttpPut("profile")]
    public async Task<ActionResult<UserDTO>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Check if email is being changed and if it's already taken
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
            {
                return Conflict(new { message = "Email already in use" });
            }
            user.Email = request.Email;
            user.EmailVerified = false; // Require re-verification if email changes
        }

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var userDto = new UserDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Ok(userDto);
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
