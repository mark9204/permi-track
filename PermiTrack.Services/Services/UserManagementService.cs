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
        // Tranzakció indítása (Biztonságos mentés)
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. Validáció
            var existingUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                if (existingUser.Username == request.Username)
                    return (false, "Username already exists", null);
                if (existingUser.Email == request.Email)
                    return (false, "Email already exists", null);
            }

            // 2. Jelszó generálás
            var password = string.IsNullOrWhiteSpace(request.Password)
                ? GenerateRandomPassword()
                : request.Password;

            var hashedPassword = _passwordHasher.HashPassword(password);

            // 3. User Létrehozása
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
            await _db.SaveChangesAsync(); // Itt kap ID-t a User

            // 4.  JAVÍTÁS: Alapértelmezett 'User' szerepkör hozzáadása
            // Így nem lesz "árva" a felhasználó
            var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole != null)
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    AssignedBy = adminUserId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await _db.SaveChangesAsync();
            }

            // 5. Audit Log
            var auditLog = new AuditLog
            {
                UserId = adminUserId,
                Action = "USER_CREATED",
                ResourceType = "Users",
                ResourceId = user.Id,
                NewValues = JsonSerializer.Serialize(new
                {
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    DefaultRole = defaultRole?.Name ?? "None"
                }),
                IpAddress = "System", // Vagy a contextből, ha elérhető lenne
                UserAgent = "System",
                CreatedAt = DateTime.UtcNow
            };

            _db.AuditLogs.Add(auditLog);
            await _db.SaveChangesAsync();

            // 6. Tranzakció lezárása
            await transaction.CommitAsync();

            // 7. Email küldés (Tranzakción kívül, mert ez lassú lehet)
            try
            {
                if (!request.EmailVerified)
                {
                    var verificationToken = Guid.NewGuid().ToString();
                    user.EmailVerificationToken = verificationToken;
                    user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                    await _db.SaveChangesAsync();
                    await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationToken);
                }
                else
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Username, password);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                // Nem dobunk hibát, mert a user már létrejött
            }

            return (true, "User created successfully", user.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating user");
            return (false, "An error occurred while creating user", null);
        }
    }

    public async Task<(bool Success, string Message, List<UserListItemDTO> Users, int TotalCount)> GetUsersAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        bool? isActive = null)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();

        // Szűrések
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

        // ✅ KRITIKUS JAVÍTÁS: Itt használjuk a Navigációs Property-ket a bonyolult Join helyett!
        // Ez fogja helyesen kitölteni a Roles listát.
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
                // Így kérjük le a szerepkör neveket egyszerűen:
                Roles = u.UserRoles
                         .Where(ur => ur.IsActive)
                         .Select(ur => ur.Role.Name)
                         .ToList()
            })
            .ToListAsync();

        return (true, "Users retrieved successfully", users, totalCount);
    }

    public async Task<(bool Success, string Message, UserDetailsDTO? User)> GetUserByIdAsync(long userId)
    {
        // Itt is optimalizálhatunk Include-dal
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return (false, "User not found", null);

        var roles = user.UserRoles
            .Where(ur => ur.IsActive)
            .Select(ur => new UserRoleDTO
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAt = ur.AssignedAt,
                ExpiresAt = ur.ExpiresAt,
                IsActive = ur.IsActive
            })
            .ToList();

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
            DirectPermissions = new List<UserPermissionDTO>()
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

        var oldValues = new
        {
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.Department
        };

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId);
            if (emailExists)
                return (false, "Email already exists");

            user.Email = request.Email;
            user.EmailVerified = false;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        if (request.Department != null) user.Department = request.Department;

        user.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "USER_UPDATED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(new { user.Email, user.FirstName, user.LastName }),
            IpAddress = "System",
            UserAgent = "System",
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

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var userRoles = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        foreach (var role in userRoles)
        {
            role.IsActive = false;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "USER_DELETED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(new { user.IsActive }),
            NewValues = JsonSerializer.Serialize(new { IsActive = false, Reason = reason }),
            IpAddress = "System",
            UserAgent = "System",
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

        if (!isActive)
        {
            var sessions = await _db.Sessions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
            foreach (var session in sessions) session.IsActive = false;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = isActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
            ResourceType = "Users",
            ResourceId = userId,
            OldValues = JsonSerializer.Serialize(new { IsActive = oldStatus }),
            NewValues = JsonSerializer.Serialize(new { IsActive = isActive, Reason = reason }),
            IpAddress = "System",
            UserAgent = "System",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return (true, $"User {(isActive ? "activated" : "deactivated")} successfully");
    }

    public async Task<BulkUserImportResult> BulkImportUsersAsync(
        BulkUserImportRequest request,
        long adminUserId)
    {
        // Ez maradhat a régi, de ha kell, itt is használhatod a Transaction-t
        var result = new BulkUserImportResult
        {
            TotalProcessed = request.Users.Count
        };
        // ... (a bulk import logika maradhat változatlan)
        // Csak a return miatt raktam ide, hogy teljes legyen a file
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