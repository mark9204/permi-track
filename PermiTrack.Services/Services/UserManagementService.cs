using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using PermiTrack.Services.Interfaces;
using System.Text.Json;

namespace PermiTrack.Services.Services;

public class UserManagementService : IUserManagementService
{
    private readonly PermiTrackDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        PermiTrackDbContext db,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ILogger<UserManagementService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, long? UserId)> CreateUserAsync(
        CreateUserRequest request,
        long adminUserId)
    {
        // Validate uniqueness
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser != null)
        {
            if (existingUser.Username == request.Username)
                return (false, "Username already exists", null);
            if (existingUser.Email == request.Email)
                return (false, "Email already exists", null);
        }

        // Generate random password if not provided
        var password = string.IsNullOrWhiteSpace(request.Password)
            ? GenerateRandomPassword()
            : request.Password;

        var hashedPassword = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Department = request.Department,
            IsActive = request.IsActive,
            EmailVerified = request.EmailVerified,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        // Audit log
        var auditLog = new AuditLog
        {
            UserId = adminUserId,
            Action = "USER_CREATED",
            ResourceType = "Users",
            ResourceId = 0, // Will be updated after SaveChanges
            NewValues = JsonSerializer.Serialize(new
            {
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.EmailVerified
            }),
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(auditLog);

        await _db.SaveChangesAsync();

        // Update audit log with actual user ID
        auditLog.ResourceId = user.Id;
        await _db.SaveChangesAsync();

        // Send welcome email
        try
        {
            if (!request.EmailVerified)
            {
                // Send verification email
                var verificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                await _db.SaveChangesAsync();

                await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationToken);
            }
            else
            {
                // Send welcome email with credentials
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Username, password);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return (true, "User created successfully", user.Id);
    }

    public async Task<(bool Success, string Message, List<UserListItemDTO> Users, int TotalCount)> GetUsersAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        bool? isActive = null)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = _db.UserRoles
                    .Where(ur => ur.UserId == u.Id && ur.IsActive)
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToList()
            })
            .ToListAsync();

        return (true, "Users retrieved successfully", users, totalCount);
    }

    public async Task<(bool Success, string Message, UserDetailsDTO? User)> GetUserByIdAsync(long userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return (false, "User not found", null);

        var roles = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new UserRoleDTO
            {
                RoleId = r.Id,
                RoleName = r.Name,
                AssignedAt = ur.AssignedAt,
                ExpiresAt = ur.ExpiresAt,
                IsActive = ur.IsActive
            })
            .ToListAsync();

        var userDetails = new UserDetailsDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Department = user.Department,
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEnd = user.LockoutEnd,
            Roles = roles,
            DirectPermissions = new List<UserPermissionDTO>() // No direct user permissions in current schema
        };

        return (true, "User details retrieved successfully", userDetails);
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(
        long userId,
        UpdateUserRequest request,
        long adminUserId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        // Store old values for audit
        var oldValues = new
        {
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.Department
        };

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check email uniqueness
            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId);
            if (emailExists)
                return (false, "Email already exists");

            user.Email = request.Email;
            user.EmailVerified = false; // Require re-verification
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.Department != null)
            user.Department = request.Department;

        user.UpdatedAt = DateTime.UtcNow;

        // Audit log
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "USER_UPDATED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(new
            {
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Department
            }),
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return (true, "User updated successfully");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(
        long userId,
        long adminUserId,
        string? reason = null)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // Deactivate all user roles
        var userRoles = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        foreach (var role in userRoles)
        {
            role.IsActive = false;
        }

        // Audit log
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "USER_DELETED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(new { user.IsActive, user.Username, user.Email }),
            NewValues = JsonSerializer.Serialize(new { IsActive = false, Reason = reason }),
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return (true, "User deleted successfully");
    }

    public async Task<(bool Success, string Message)> SetUserActiveStatusAsync(
        long userId,
        bool isActive,
        long adminUserId,
        string? reason = null)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (user.IsActive == isActive)
            return (false, $"User is already {(isActive ? "active" : "inactive")}");

        var oldStatus = user.IsActive;
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        // If deactivating, also deactivate all sessions
        if (!isActive)
        {
            var sessions = await _db.Sessions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
            foreach (var session in sessions)
            {
                session.IsActive = false;
            }
        }

        // Audit log
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = isActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(new { IsActive = oldStatus }),
            NewValues = JsonSerializer.Serialize(new { IsActive = isActive, Reason = reason }),
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return (true, $"User {(isActive ? "activated" : "deactivated")} successfully");
    }

    public async Task<BulkUserImportResult> BulkImportUsersAsync(
        BulkUserImportRequest request,
        long adminUserId)
    {
        var result = new BulkUserImportResult
        {
            TotalProcessed = request.Users.Count
        };

        for (int i = 0; i < request.Users.Count; i++)
        {
            var userItem = request.Users[i];

            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(userItem.Username) || string.IsNullOrWhiteSpace(userItem.Email))
                {
                    result.Errors.Add(new BulkUserImportError
                    {
                        LineNumber = i + 1,
                        Username = userItem.Username,
                        Email = userItem.Email,
                        ErrorMessage = "Username and Email are required"
                    });
                    result.FailureCount++;
                    continue;
                }

                // Check duplicates
                var exists = await _db.Users.AnyAsync(u => u.Username == userItem.Username || u.Email == userItem.Email);
                if (exists)
                {
                    result.Errors.Add(new BulkUserImportError
                    {
                        LineNumber = i + 1,
                        Username = userItem.Username,
                        Email = userItem.Email,
                        ErrorMessage = "Username or Email already exists"
                    });
                    result.FailureCount++;
                    continue;
                }

                // Create user
                var password = GenerateRandomPassword();
                var user = new User
                {
                    Username = userItem.Username,
                    Email = userItem.Email,
                    PasswordHash = _passwordHasher.HashPassword(password),
                    FirstName = userItem.FirstName,
                    LastName = userItem.LastName,
                    PhoneNumber = userItem.PhoneNumber,
                    Department = userItem.Department,
                    IsActive = true,
                    EmailVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Assign roles if specified
                if (userItem.RoleNames != null && userItem.RoleNames.Any())
                {
                    var roles = await _db.Roles.Where(r => userItem.RoleNames.Contains(r.Name)).ToListAsync();
                    foreach (var role in roles)
                    {
                        _db.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            AssignedAt = DateTime.UtcNow,
                            AssignedBy = adminUserId,
                            IsActive = true
                        });
                    }
                    await _db.SaveChangesAsync();
                }

                // Send welcome email
                if (request.SendWelcomeEmail)
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.Username, password);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                    }
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing user {Username}", userItem.Username);
                result.Errors.Add(new BulkUserImportError
                {
                    LineNumber = i + 1,
                    Username = userItem.Username,
                    Email = userItem.Email,
                    ErrorMessage = ex.Message
                });
                result.FailureCount++;
            }
        }

        // Audit log
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "BULK_USER_IMPORT",
            ResourceType = "Users",
            ResourceId = 0,
            NewValues = JsonSerializer.Serialize(new
            {
                result.TotalProcessed,
                result.SuccessCount,
                result.FailureCount
            }),
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return result;
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
