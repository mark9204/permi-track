using PermiTrack.DataContext.DTOs;

namespace PermiTrack.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterRequest request);
    Task<AuthResponseDTO> LoginAsync(LoginRequest request);
    Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task LogoutAsync(string refreshToken);
}
