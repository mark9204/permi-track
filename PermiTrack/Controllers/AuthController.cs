using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermiTrack.DataContext.DTOs;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDTO>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(token);
            
            if (result)
            {
                return Ok(new { message = "Email verified successfully" });
            }
            
            return BadRequest(new { message = "Invalid or expired verification token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during email verification" });
        }
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.RequestPasswordResetAsync(request.Email);
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            
            if (result)
            {
                return Ok(new { message = "Password reset successfully" });
            }
            
            return BadRequest(new { message = "Invalid or expired reset token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during password reset" });
        }
    }
}
