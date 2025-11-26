using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

public class AuthService : IAuthService
{
    private readonly PermiTrackDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(
        PermiTrackDbContext db,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IMapper mapper)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequest request)
    {
        // Check if username exists
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email exists
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create new user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailVerified = false,
            EmailVerificationToken = _tokenService.GenerateEmailVerificationToken(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            FailedLoginAttempts = 0
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Send verification email
        await _emailService.SendEmailVerificationAsync(
            user.Email,
            user.Username,
            user.EmailVerificationToken
        );

        // Get user roles and permissions
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token in session
        var session = new Sessions
        {
            UserId = user.Id,
            TokenHash = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            LastActivity = DateTime.UtcNow,
            IpAddress = string.Empty,
            UserAgent = string.Empty
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = _mapper.Map<UserDTO>(user)
        };
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequest request)
    {
        // Find user by username
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            
            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }

            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is not active");
        }

        // Reset failed login attempts
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        user.LockoutEnd = null;

        // Get user roles and permissions
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token in session
        var session = new Sessions
        {
            UserId = user.Id,
            TokenHash = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            LastActivity = DateTime.UtcNow,
            IpAddress = string.Empty,
            UserAgent = string.Empty
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = _mapper.Map<UserDTO>(user)
        };
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        // Find active session with the refresh token
        var session = await _db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == refreshToken && s.IsActive);

        if (session == null || session.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Update last activity
        session.LastActivity = DateTime.UtcNow;

        var user = session.User;

        // Get user roles and permissions
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // Generate new access token
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);

        await _db.SaveChangesAsync();

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = _mapper.Map<UserDTO>(user)
        };
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null)
        {
            return false;
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Don't reveal that user doesn't exist for security
            return true;
        }

        user.PasswordResetToken = _tokenService.GeneratePasswordResetToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _emailService.SendPasswordResetAsync(
            user.Email,
            user.Username,
            user.PasswordResetToken
        );

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

        if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        // Invalidate all active sessions
        var sessions = await _db.Sessions
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.TokenHash == refreshToken && s.IsActive);

        if (session != null)
        {
            session.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    private async Task<IEnumerable<string>> GetUserRolesAsync(long userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    private async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToListAsync();
    }
}
