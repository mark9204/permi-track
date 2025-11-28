using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.Enums;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.BackgroundJobs;

/// <summary>
/// Background service for automatic role expiration handling
/// SPEC 12: Maintenance - Automatic Role Expiration
/// 
/// This service runs periodically to:
/// 1. Find expired user roles (ExpiresAt < now && IsActive)
/// 2. Deactivate them (set IsActive = false)
/// 3. Send notifications to affected users
/// </summary>
public class RoleExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoleExpirationJob> _logger;
    private readonly TimeSpan _checkInterval;

    public RoleExpirationJob(
        IServiceProvider serviceProvider,
        ILogger<RoleExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // SPEC 12: Check interval configuration
        // Production: 1 hour (TimeSpan.FromHours(1))
        // Testing: 1 minute (TimeSpan.FromMinutes(1))
        _checkInterval = TimeSpan.FromHours(1);  // ← Change to TimeSpan.FromMinutes(1) for testing
    }

    /// <summary>
    /// SPEC 12: Main execution loop for role expiration checking
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SPEC 12: Role Expiration Job started. Check interval: {Interval}",
            _checkInterval);

        // Use PeriodicTimer for accurate periodic execution
        using var timer = new PeriodicTimer(_checkInterval);

        try
        {
            // Run immediately on startup, then periodically
            await ProcessExpiredRolesAsync(stoppingToken);

            // Continue running periodically
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessExpiredRolesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SPEC 12: Role Expiration Job is stopping (cancellation requested)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPEC 12: Fatal error in Role Expiration Job");
            throw;
        }
    }

    /// <summary>
    /// SPEC 12: Process expired roles - find, deactivate, and notify users
    /// </summary>
    private async Task ProcessExpiredRolesAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SPEC 12: Starting role expiration check at {Time}", DateTime.UtcNow);

        // SPEC 12: Create a new service scope to access scoped services
        // This is necessary because BackgroundService is a singleton
        using var scope = _serviceProvider.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<PermiTrackDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var now = DateTime.UtcNow;

            // SPEC 12: Find all expired user roles that are still active
            var expiredRoles = await context.UserRoles
                .Where(ur => ur.ExpiresAt.HasValue &&
                            ur.ExpiresAt.Value < now &&
                            ur.IsActive)
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync(stoppingToken);

            if (expiredRoles.Count == 0)
            {
                _logger.LogInformation("SPEC 12: No expired roles found");
                return;
            }

            _logger.LogInformation(
                "SPEC 12: Found {Count} expired roles to process",
                expiredRoles.Count);

            var deactivatedCount = 0;
            var notificationsSent = 0;
            var notificationsFailed = 0;

            foreach (var userRole in expiredRoles)
            {
                try
                {
                    // SPEC 12: Deactivate the expired role
                    userRole.IsActive = false;
                    deactivatedCount++;

                    _logger.LogInformation(
                        "SPEC 12: Deactivated expired role - User: {Username} (ID: {UserId}), Role: {RoleName} (ID: {RoleId}), Expired: {ExpiredAt}",
                        userRole.User.Username,
                        userRole.UserId,
                        userRole.Role.Name,
                        userRole.RoleId,
                        userRole.ExpiresAt);

                    // SPEC 12: Send notification to user about role expiration
                    try
                    {
                        await notificationService.SendNotificationAsync(
                            userId: userRole.UserId,
                            title: "Role Access Expired ⏰",
                            message: $"Your access to the '{userRole.Role.Name}' role has expired and has been automatically deactivated.",
                            type: NotificationType.Warning,
                            relatedResourceType: "UserRole",
                            relatedResourceId: userRole.Id);

                        notificationsSent++;

                        _logger.LogInformation(
                            "SPEC 12: Expiration notification sent to user {UserId} for role {RoleName}",
                            userRole.UserId,
                            userRole.Role.Name);
                    }
                    catch (Exception notificationEx)
                    {
                        // Don't fail the job if notification fails
                        notificationsFailed++;
                        _logger.LogError(notificationEx,
                            "SPEC 12: Failed to send expiration notification to user {UserId} for role {RoleName}",
                            userRole.UserId,
                            userRole.Role.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "SPEC 12: Error processing expired role for user {UserId}, role {RoleId}",
                        userRole.UserId,
                        userRole.RoleId);
                }
            }

            // SPEC 12: Save all deactivations to database
            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "SPEC 12: Role expiration check completed - Deactivated: {Deactivated}, Notifications sent: {Sent}, Failed: {Failed}",
                deactivatedCount,
                notificationsSent,
                notificationsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPEC 12: Error during role expiration processing");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SPEC 12: Role Expiration Job is stopping");
        return base.StopAsync(cancellationToken);
    }
}
