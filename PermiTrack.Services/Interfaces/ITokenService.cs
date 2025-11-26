using PermiTrack.DataContext.Entites;

namespace PermiTrack.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string GenerateEmailVerificationToken();
    string GeneratePasswordResetToken();
}
