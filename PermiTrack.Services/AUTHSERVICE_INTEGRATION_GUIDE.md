# AuthService Integration with SecurityService

## How to Integrate Login Tracking (SPEC 7: Security)

### 1. Update Constructor

Add `ISecurityService` to the constructor parameters:

```csharp
public class AuthService : IAuthService
{
    private readonly PermiTrackDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ISecurityService _securityService;  // ? ADD THIS

    public AuthService(
        PermiTrackDbContext db,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IMapper mapper,
        ISecurityService securityService)  // ? ADD THIS PARAMETER
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _mapper = mapper;
        _securityService = securityService;  // ? STORE IT
    }
```

### 2. Update LoginAsync Method

Add two additional parameters to accept IP and UserAgent:

```csharp
public async Task<AuthResponseDTO> LoginAsync(
    LoginRequest request, 
    string ipAddress,      // ? ADD THIS
    string userAgent)      // ? ADD THIS
{
    // Find user by username
    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username);

    // SPEC 7: CASE 1 - User Not Found
    if (user == null)
    {
        // Record failed login attempt BEFORE throwing exception
        await _securityService.RecordLoginAttemptAsync(
            userName: request.Username,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: false,
            userId: null,
            failureReason: "UserNotFound");

        throw new UnauthorizedAccessException("Invalid username or password");
    }

    // SPEC 7: CASE 2 - Account Locked
    if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
    {
        // Record failed login attempt
        await _securityService.RecordLoginAttemptAsync(
            userName: request.Username,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: false,
            userId: user.Id,
            failureReason: "AccountLocked");

        throw new UnauthorizedAccessException("Account is locked. Please try again later.");
    }

    // SPEC 7: CASE 3 - Invalid Password
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

        // Record failed login attempt
        await _securityService.RecordLoginAttemptAsync(
            userName: request.Username,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: false,
            userId: user.Id,
            failureReason: "InvalidPassword");

        throw new UnauthorizedAccessException("Invalid username or password");
    }

    // SPEC 7: CASE 4 - Account Inactive
    if (!user.IsActive)
    {
        // Record failed login attempt
        await _securityService.RecordLoginAttemptAsync(
            userName: request.Username,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: false,
            userId: user.Id,
            failureReason: "AccountInactive");

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
        IpAddress = ipAddress,      // ? USE PARAMETER
        UserAgent = userAgent        // ? USE PARAMETER
    };

    _db.Sessions.Add(session);
    await _db.SaveChangesAsync();

    // SPEC 7: CASE 5 - Successful Login
    // Record successful login attempt AFTER all checks pass
    await _securityService.RecordLoginAttemptAsync(
        userName: request.Username,
        ipAddress: ipAddress,
        userAgent: userAgent,
        isSuccess: true,
        userId: user.Id,
        failureReason: null);

    return new AuthResponseDTO
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        User = _mapper.Map<UserDTO>(user),
        Roles = roles,
        Permissions = permissions
    };
}
```

### 3. Update IAuthService Interface

Update the interface to match:

```csharp
public interface IAuthService
{
    // ... other methods ...
    
    Task<AuthResponseDTO> LoginAsync(
        LoginRequest request, 
        string ipAddress,     // ? ADD THIS
        string userAgent);    // ? ADD THIS
    
    // ... other methods ...
}
```

### 4. Update AuthController

Update the controller to pass IP and UserAgent:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    try
    {
        // Extract IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        // Extract User Agent
        var userAgent = Request.Headers["User-Agent"].ToString();
        
        // Call login with IP and UserAgent
        var response = await _authService.LoginAsync(request, ipAddress, userAgent);
        
        return Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Unauthorized(new { message = "Invalid credentials" });
    }
}
```

## Summary of Integration Points

### Failed Login Cases (Tracked):
1. ? User Not Found - `failureReason: "UserNotFound"`
2. ? Account Locked - `failureReason: "AccountLocked"`
3. ? Invalid Password - `failureReason: "InvalidPassword"`
4. ? Account Inactive - `failureReason: "AccountInactive"`

### Successful Login:
5. ? Successful - `isSuccess: true, failureReason: null`

All login attempts are recorded in the `LoginAttempts` table for security monitoring and analysis.
