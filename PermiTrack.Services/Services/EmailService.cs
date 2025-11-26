using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailVerificationAsync(string email, string username, string verificationToken)
    {
        var subject = "Verify your email address";
        var verificationUrl = $"{_configuration["AppUrl"]}/verify-email?token={verificationToken}";
        var body = $@"
            <h2>Welcome to PermiTrack, {username}!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <a href='{verificationUrl}'>Verify Email</a>
            <p>This link will expire in 24 hours.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string username, string resetToken)
    {
        var subject = "Reset your password";
        var resetUrl = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Hello {username},</p>
            <p>You requested to reset your password. Click the link below to proceed:</p>
            <a href='{resetUrl}'>Reset Password</a>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request this, please ignore this email.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var subject = "Welcome to PermiTrack";
        var body = $@"
            <h2>Welcome to PermiTrack, {username}!</h2>
            <p>Your account has been successfully created.</p>
            <p>You can now log in and start managing permissions.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:FromName"],
            _configuration["Email:FromAddress"]
        ));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Email:SmtpServer"],
            int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _configuration["Email:Username"],
            _configuration["Email:Password"]
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
