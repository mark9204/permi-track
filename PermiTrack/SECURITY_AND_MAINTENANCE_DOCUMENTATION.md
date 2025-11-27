# Security and Maintenance Modules Documentation

## Overview

This document covers two critical system modules:
1. **SPEC 7: Security - Login Tracking**
2. **SPEC 12: Maintenance - Automatic Role Expiration**

---

## SPEC 7: Security - Login Tracking

### Purpose

Track all login attempts (successful and failed) for security monitoring, brute force detection, and audit compliance.

### Components

#### 1. LoginAttempt Entity

**Location:** `PermiTrack.DataContext\Entites\LoginAttempt.cs`

```csharp
public class LoginAttempt
{
    public long Id { get; set; }
    public long? UserId { get; set; }           // Null if user doesn't exist
    public string UserName { get; set; }        // Username entered in login form
    public string IpAddress { get; set; }       // IP address of attempt
    public string UserAgent { get; set; }       // Browser/client information
    public bool IsSuccess { get; set; }         // Success or failure
    public string? FailureReason { get; set; }  // Reason if failed
    public DateTime AttemptedAt { get; set; }   // When attempt occurred
    
    public User? User { get; set; }
}
```

#### 2. ISecurityService

**Location:** `PermiTrack.Services\Interfaces\ISecurityService.cs`

**Methods:**
- `RecordLoginAttemptAsync()` - Record a login attempt
- `GetRecentFailedAttemptsAsync()` - Get failed attempts for a user
- `GetFailedAttemptsByIpAsync()` - Get failed attempts from an IP (brute force detection)
- `GetUserLoginHistoryAsync()` - Get login history for a user

#### 3. SecurityService Implementation

**Location:** `PermiTrack.Services\Services\SecurityService.cs`

Provides secure login tracking with comprehensive logging.

### Integration with AuthService

#### Constructor Update

```csharp
public AuthService(
    PermiTrackDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    IMapper mapper,
    ISecurityService securityService)  // ? ADD THIS
{
    _securityService = securityService;
}
```

#### Login Method Update

Add two parameters: `ipAddress` and `userAgent`

```csharp
public async Task<AuthResponseDTO> LoginAsync(
    LoginRequest request,
    string ipAddress,    // ? ADD
    string userAgent)    // ? ADD
```

#### Tracking Points

**1. User Not Found**
```csharp
if (user == null)
{
    await _securityService.RecordLoginAttemptAsync(
        userName: request.Username,
        ipAddress: ipAddress,
        userAgent: userAgent,
        isSuccess: false,
        userId: null,
        failureReason: "UserNotFound");
    
    throw new UnauthorizedAccessException(...);
}
```

**2. Account Locked**
```csharp
if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
{
    await _securityService.RecordLoginAttemptAsync(
        userName: request.Username,
        ipAddress: ipAddress,
        userAgent: userAgent,
        isSuccess: false,
        userId: user.Id,
        failureReason: "AccountLocked");
    
    throw new UnauthorizedAccessException(...);
}
```

**3. Invalid Password**
```csharp
if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
{
    user.FailedLoginAttempts++;
    if (user.FailedLoginAttempts >= 5)
    {
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
    }
    await _db.SaveChangesAsync();
    
    await _securityService.RecordLoginAttemptAsync(
        userName: request.Username,
        ipAddress: ipAddress,
        userAgent: userAgent,
        isSuccess: false,
        userId: user.Id,
        failureReason: "InvalidPassword");
    
    throw new UnauthorizedAccessException(...);
}
```

**4. Account Inactive**
```csharp
if (!user.IsActive)
{
    await _securityService.RecordLoginAttemptAsync(
        userName: request.Username,
        ipAddress: ipAddress,
        userAgent: userAgent,
        isSuccess: false,
        userId: user.Id,
        failureReason: "AccountInactive");
    
    throw new UnauthorizedAccessException(...);
}
```

**5. Successful Login**
```csharp
// After all checks pass and tokens are generated
await _securityService.RecordLoginAttemptAsync(
    userName: request.Username,
    ipAddress: ipAddress,
    userAgent: userAgent,
    isSuccess: true,
    userId: user.Id,
    failureReason: null);
```

#### Controller Update

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Extract IP address
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    
    // Extract User Agent
    var userAgent = Request.Headers["User-Agent"].ToString();
    
    // Pass to AuthService
    var response = await _authService.LoginAsync(request, ipAddress, userAgent);
    
    return Ok(response);
}
```

### Use Cases

#### 1. Security Monitoring

**Detect Brute Force Attacks:**
```csharp
var failedAttempts = await _securityService.GetFailedAttemptsByIpAsync("192.168.1.100", 24);
if (failedAttempts.Count() > 20)
{
    // Block this IP or alert security team
}
```

**Monitor User Account:**
```csharp
var recentFailures = await _securityService.GetRecentFailedAttemptsAsync("john.doe", 24);
if (recentFailures.Count() > 5)
{
    // Notify user of suspicious activity
}
```

#### 2. User Login History

```csharp
var loginHistory = await _securityService.GetUserLoginHistoryAsync(userId, 50);
// Display in user profile or security dashboard
```

#### 3. Compliance & Audit

All login attempts are permanently stored for:
- Security audits
- Compliance requirements (SOC 2, ISO 27001)
- Forensic analysis
- User activity reports

### Database Schema

```sql
CREATE TABLE LoginAttempts (
    Id BIGINT PRIMARY KEY IDENTITY,
    UserId BIGINT NULL,
    UserName NVARCHAR(100) NOT NULL,
    IpAddress NVARCHAR(50) NOT NULL,
    UserAgent NVARCHAR(500) NOT NULL,
    IsSuccess BIT NOT NULL,
    FailureReason NVARCHAR(200) NULL,
    AttemptedAt DATETIME2 NOT NULL,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    
    INDEX IX_LoginAttempts_AttemptedAt (AttemptedAt),
    INDEX IX_LoginAttempts_IpAddress (IpAddress),
    INDEX IX_LoginAttempts_UserName (UserName),
    INDEX IX_LoginAttempts_IsSuccess_AttemptedAt (IsSuccess, AttemptedAt)
);
```

---

## SPEC 12: Maintenance - Automatic Role Expiration

### Purpose

Automatically deactivate expired user roles and notify affected users. This ensures that temporary access grants are properly enforced without manual intervention.

### Components

#### 1. RoleExpirationJob

**Location:** `BackgroundJobs\RoleExpirationJob.cs`

A background service that runs periodically to process expired roles.

**Key Features:**
- Inherits from `BackgroundService`
- Uses `PeriodicTimer` for accurate scheduling
- Creates service scopes for DbContext access (Singleton to Scoped)
- Finds expired roles (`ExpiresAt < now && IsActive`)
- Deactivates roles (sets `IsActive = false`)
- Sends notifications to users
- Comprehensive error handling and logging

#### 2. Configuration

**Check Interval:**
- **Production:** 1 hour (`TimeSpan.FromHours(1)`)
- **Testing:** 1 minute (`TimeSpan.FromMinutes(1)`)

Change in constructor:
```csharp
_checkInterval = TimeSpan.FromHours(1);  // Production
// or
_checkInterval = TimeSpan.FromMinutes(1);  // Testing
```

### How It Works

#### 1. Service Startup

```csharp
// Registered in Program.cs as a Hosted Service
builder.Services.AddHostedService<RoleExpirationJob>();
```

When the application starts, the job:
1. Runs immediately (checks for expired roles right away)
2. Sets up periodic timer
3. Runs at configured intervals

#### 2. Execution Flow

```
Application Starts
       ?
RoleExpirationJob.ExecuteAsync() called
       ?
Process expired roles immediately
       ?
Wait for timer interval
       ?
Process expired roles again
       ?
Repeat until application stops
```

#### 3. Processing Logic

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var timer = new PeriodicTimer(_checkInterval);
    
    try
    {
        // Run immediately
        await ProcessExpiredRolesAsync(stoppingToken);
        
        // Run periodically
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessExpiredRolesAsync(stoppingToken);
        }
    }
    catch (OperationCanceledException)
    {
        // Normal shutdown
    }
}
```

#### 4. Role Processing

```csharp
private async Task ProcessExpiredRolesAsync(CancellationToken stoppingToken)
{
    // Create scope (BackgroundService is Singleton, DbContext is Scoped)
    using var scope = _serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PermiTrackDbContext>();
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
    
    // Find expired roles
    var expiredRoles = await context.UserRoles
        .Where(ur => ur.ExpiresAt.HasValue &&
                    ur.ExpiresAt.Value < DateTime.UtcNow &&
                    ur.IsActive)
        .Include(ur => ur.User)
        .Include(ur => ur.Role)
        .ToListAsync(stoppingToken);
    
    foreach (var userRole in expiredRoles)
    {
        // Deactivate role
        userRole.IsActive = false;
        
        // Send notification
        await notificationService.SendNotificationAsync(
            userRole.UserId,
            "Role Access Expired ?",
            $"Your access to the '{userRole.Role.Name}' role has expired.",
            NotificationType.Warning,
            "UserRole",
            userRole.Id);
    }
    
    // Save all changes
    await context.SaveChangesAsync(stoppingToken);
}
```

### Service Scope Pattern

**Why Service Scopes?**

```csharp
// ? WRONG - BackgroundService is Singleton, can't inject Scoped services directly
public RoleExpirationJob(PermiTrackDbContext context)  // ERROR!

// ? CORRECT - Inject IServiceProvider, create scope when needed
public RoleExpirationJob(IServiceProvider serviceProvider)
{
    _serviceProvider = serviceProvider;
}

// In method:
using var scope = _serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<PermiTrackDbContext>();
```

**Lifetime Rules:**
- `BackgroundService` = Singleton
- `DbContext` = Scoped (per request)
- `INotificationService` = Scoped

**Solution:** Create a new scope for each processing cycle.

### Notification Integration

When a role expires, the user receives:

```
Title: "Role Access Expired ?"
Message: "Your access to the 'UserManager' role has expired and has been 
          automatically deactivated."
Type: Warning
RelatedResourceType: "UserRole"
RelatedResourceId: {userRoleId}
```

### Error Handling

**Graceful Degradation:**
```csharp
try
{
    // Deactivate role
    userRole.IsActive = false;
    
    // Try to send notification
    try
    {
        await notificationService.SendNotificationAsync(...);
    }
    catch (Exception notificationEx)
    {
        // Log but continue processing other roles
        _logger.LogError(notificationEx, "Failed to send notification");
    }
}
catch (Exception ex)
{
    // Log but continue processing other roles
    _logger.LogError(ex, "Error processing expired role");
}
```

**Key Principles:**
- Notification failures don't stop role deactivation
- Individual role failures don't stop batch processing
- All errors are logged for investigation
- Application continues running even if job fails

### Logging

Comprehensive logging at every step:

```
[INFO] SPEC 12: Role Expiration Job started. Check interval: 01:00:00
[INFO] SPEC 12: Starting role expiration check at 2024-01-15 10:00:00
[INFO] SPEC 12: Found 3 expired roles to process
[INFO] SPEC 12: Deactivated expired role - User: john.doe, Role: TempAdmin
[INFO] SPEC 12: Expiration notification sent to user 10 for role TempAdmin
[INFO] SPEC 12: Role expiration check completed - Deactivated: 3, Notifications sent: 3, Failed: 0
```

### Testing

#### 1. Set Short Interval
```csharp
_checkInterval = TimeSpan.FromMinutes(1);  // For testing
```

#### 2. Create Test Data
```sql
-- Create a user role that expires in 2 minutes
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, ExpiresAt, IsActive)
VALUES (10, 5, GETUTCDATE(), DATEADD(MINUTE, 2, GETUTCDATE()), 1);
```

#### 3. Watch Logs
```
Wait 2+ minutes...
Check application logs for:
- "Starting role expiration check"
- "Found X expired roles"
- "Deactivated expired role"
- "Expiration notification sent"
```

#### 4. Verify Database
```sql
-- Check that role is now inactive
SELECT * FROM UserRoles WHERE Id = {roleId};
-- IsActive should be 0

-- Check notification was created
SELECT * FROM Notifications WHERE UserId = 10 ORDER BY CreatedAt DESC;
```

### Registration

**In Program.cs:**
```csharp
// SPEC 12: Register Role Expiration Background Job
builder.Services.AddHostedService<RoleExpirationJob>();
```

This automatically:
- Starts the service when application starts
- Stops the service when application stops
- Runs in background without blocking requests

### Production Considerations

#### 1. Performance

- Runs every hour (configurable)
- Processes only expired, active roles
- Efficient database queries with indexes
- Minimal impact on application performance

#### 2. Reliability

- Automatic retry on next cycle if failure occurs
- Comprehensive error handling
- Detailed logging for troubleshooting
- Graceful shutdown support

#### 3. Monitoring

**Metrics to Track:**
- Number of roles expired per cycle
- Processing duration
- Notification success/failure rate
- Any errors in logs

**Alerts to Configure:**
- Job hasn't run in X hours
- High failure rate for notifications
- Errors in processing

#### 4. Scaling

- Safe to run on single instance (no conflicts)
- Can run on multiple instances (idempotent)
- Database handles concurrency with row locking

### Database Impact

**Query Pattern:**
```sql
-- Executed every check cycle
SELECT *
FROM UserRoles ur
JOIN Users u ON ur.UserId = u.Id
JOIN Roles r ON ur.RoleId = r.Id
WHERE ur.ExpiresAt IS NOT NULL
  AND ur.ExpiresAt < GETUTCDATE()
  AND ur.IsActive = 1;
```

**Indexes:**
- Composite index on `(ExpiresAt, IsActive)` recommended
- Will be very fast even with millions of roles

**Updates:**
```sql
-- For each expired role
UPDATE UserRoles
SET IsActive = 0
WHERE Id = @Id;

-- Plus notification insert
INSERT INTO Notifications (...) VALUES (...);
```

---

## Integration Checklist

### SPEC 7: Login Tracking

- [ ] `LoginAttempt` entity created
- [ ] DbContext updated with `LoginAttempts` DbSet
- [ ] Entity configuration added with indexes
- [ ] `ISecurityService` interface created
- [ ] `SecurityService` implementation created
- [ ] Service registered in `Program.cs`
- [ ] `IAuthService` interface updated with `ipAddress` and `userAgent` parameters
- [ ] `AuthService` updated to inject `ISecurityService`
- [ ] `AuthService.LoginAsync()` updated to record all login attempts
- [ ] `AuthController` updated to pass IP and User Agent
- [ ] Database migration created and applied
- [ ] Testing completed

### SPEC 12: Automatic Role Expiration

- [ ] `RoleExpirationJob` created
- [ ] Service registered with `AddHostedService` in `Program.cs`
- [ ] Check interval configured (1 hour for production)
- [ ] Notification integration verified
- [ ] Error handling tested
- [ ] Logging verified
- [ ] Database indexes created
- [ ] Tested with short interval
- [ ] Monitoring configured
- [ ] Documentation completed

---

## Database Migrations

### Create Migration

```bash
dotnet ef migrations add AddSecurityAndMaintenanceModules --project ..\PermiTrack.DataContext --startup-project .
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

---

## Summary

### SPEC 7: Security - Login Tracking ?

- **Purpose:** Track all login attempts for security monitoring
- **Integration:** AuthService records every login attempt
- **Benefits:** Brute force detection, audit compliance, user activity history
- **Storage:** Permanent record in `LoginAttempts` table

### SPEC 12: Maintenance - Automatic Role Expiration ?

- **Purpose:** Automatically deactivate expired roles
- **Execution:** Background job runs every hour
- **Process:** Find expired roles ? Deactivate ? Notify users
- **Benefits:** Enforces temporary access, reduces manual work, improves security

Both modules are production-ready with comprehensive error handling, logging, and monitoring capabilities.
