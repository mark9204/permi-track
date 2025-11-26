namespace PermiTrack.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string username, string verificationToken);
    Task SendPasswordResetAsync(string email, string username, string resetToken);
    Task SendWelcomeEmailAsync(string email, string username);
}
