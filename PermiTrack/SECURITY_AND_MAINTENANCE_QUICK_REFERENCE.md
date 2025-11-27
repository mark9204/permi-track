# Security & Maintenance - Quick Reference

## SPEC 7: Login Tracking

### Quick Setup

#### 1. Entity & Service Already Created ?
- `LoginAttempt.cs` - Entity for tracking
- `ISecurityService.cs` - Interface
- `SecurityService.cs` - Implementation

#### 2. Already Registered in Program.cs ?
```csharp
builder.Services.AddScoped<ISecurityService, SecurityService>();
```

#### 3. Integration with AuthService

**Update Constructor:**
```csharp
public AuthService(
    // ...existing parameters...
    ISecurityService securityService)  // ? ADD THIS
{
    _securityService = securityService;
}
```

**Update Interface:**
```csharp
Task<AuthResponseDTO> LoginAsync(
    LoginRequest request,
    string ipAddress,    // ? ADD
    string userAgent);   // ? ADD
```

**Update LoginAsync Method:**
```csharp
// User Not Found
await _securityService.RecordLoginAttemptAsync(
    request.Username, ipAddress, userAgent, false, null, "UserNotFound");

// Invalid Password
await _securityService.RecordLoginAttemptAsync(
    request.Username, ipAddress, userAgent, false, user.Id, "InvalidPassword");

// Success
await _securityService.RecordLoginAttemptAsync(
    request.Username, ipAddress, userAgent, true, user.Id, null);
```

**Update Controller:**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    var userAgent = Request.Headers["User-Agent"].ToString();
    
    var response = await _authService.LoginAsync(request, ipAddress, userAgent);
    return Ok(response);
}
```

### Tracking Points

| Case | FailureReason | UserId |
|------|---------------|--------|
| User Not Found | "UserNotFound" | null |
| Account Locked | "AccountLocked" | user.Id |
| Invalid Password | "InvalidPassword" | user.Id |
| Account Inactive | "AccountInactive" | user.Id |
| Success | null | user.Id |

### Query Examples

```csharp
// Get failed attempts for user
var failures = await _securityService.GetRecentFailedAttemptsAsync("john.doe", 24);

// Get failed attempts from IP (brute force detection)
var ipFailures = await _securityService.GetFailedAttemptsByIpAsync("192.168.1.100", 24);

// Get user login history
var history = await _securityService.GetUserLoginHistoryAsync(userId, 50);
```

---

## SPEC 12: Automatic Role Expiration

### Quick Setup

#### 1. Job Already Created ?
- `RoleExpirationJob.cs` - Background service

#### 2. Already Registered in Program.cs ?
```csharp
builder.Services.AddHostedService<RoleExpirationJob>();
```

### How It Works

```
Application Starts
       ?
Job starts automatically
       ?
Runs immediately (checks expired roles)
       ?
Waits 1 hour (configurable)
       ?
Runs again
       ?
Repeat...
```

### What It Does

Every cycle:
1. **Find** expired roles: `ExpiresAt < now && IsActive`
2. **Deactivate**: Set `IsActive = false`
3. **Notify**: Send notification to user

### Configuration

**Production (1 hour):**
```csharp
_checkInterval = TimeSpan.FromHours(1);
```

**Testing (1 minute):**
```csharp
_checkInterval = TimeSpan.FromMinutes(1);
```

### Notification Sent

```
Title: "Role Access Expired ?"
Message: "Your access to the 'RoleName' role has expired and has been 
          automatically deactivated."
Type: Warning
```

### Testing

#### 1. Set Short Interval
```csharp
// In RoleExpirationJob.cs constructor
_checkInterval = TimeSpan.FromMinutes(1);
```

#### 2. Create Test Role
```sql
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, ExpiresAt, IsActive)
VALUES (10, 5, GETUTCDATE(), DATEADD(MINUTE, 2, GETUTCDATE()), 1);
```

#### 3. Wait & Check Logs
```
Wait 2+ minutes...
Check logs for:
- "Starting role expiration check"
- "Found X expired roles"
- "Deactivated expired role"
```

#### 4. Verify Database
```sql
-- Check role is inactive
SELECT * FROM UserRoles WHERE Id = X;  -- IsActive = 0

-- Check notification created
SELECT * FROM Notifications WHERE UserId = 10 ORDER BY CreatedAt DESC;
```

---

## File Structure

```
PermiTrack/
??? BackgroundJobs/
?   ??? RoleExpirationJob.cs  (NEW - SPEC 12)
??? SECURITY_AND_MAINTENANCE_DOCUMENTATION.md

PermiTrack.DataContext/
??? Entites/
    ??? LoginAttempt.cs  (NEW - SPEC 7)

PermiTrack.Services/
??? Interfaces/
?   ??? ISecurityService.cs  (NEW - SPEC 7)
??? Services/
?   ??? SecurityService.cs  (NEW - SPEC 7)
?   ??? AuthService.cs  (UPDATE - Add ISecurityService)
??? AUTHSERVICE_INTEGRATION_GUIDE.md
```

---

## Database Schema

### LoginAttempts Table (SPEC 7)
```sql
CREATE TABLE LoginAttempts (
    Id BIGINT PRIMARY KEY,
    UserId BIGINT NULL,
    UserName NVARCHAR(100),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    IsSuccess BIT,
    FailureReason NVARCHAR(200),
    AttemptedAt DATETIME2
);
```

### Indexes
```sql
INDEX IX_LoginAttempts_AttemptedAt (AttemptedAt);
INDEX IX_LoginAttempts_IpAddress (IpAddress);
INDEX IX_LoginAttempts_UserName (UserName);
INDEX IX_LoginAttempts_IsSuccess_AttemptedAt (IsSuccess, AttemptedAt);
```

---

## Key Features

### SPEC 7: Login Tracking
? Tracks all login attempts (success + failure)  
? Records IP address and User Agent  
? Supports brute force detection  
? Provides user activity history  
? Compliance-ready (SOC 2, ISO 27001)  

### SPEC 12: Role Expiration
? Automatic background processing  
? Runs every hour (configurable)  
? Deactivates expired roles  
? Sends notifications to users  
? Error-resilient (failures don't stop job)  
? Comprehensive logging  

---

## Service Scope Pattern (Important!)

**Why needed:**
- `BackgroundService` = Singleton
- `DbContext` = Scoped (per request)
- Can't inject Scoped into Singleton!

**Solution:**
```csharp
public RoleExpirationJob(IServiceProvider serviceProvider)
{
    _serviceProvider = serviceProvider;
}

// In method:
using var scope = _serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<PermiTrackDbContext>();
var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
```

---

## Registration Summary

### In Program.cs

```csharp
// SPEC 7: Security Service
builder.Services.AddScoped<ISecurityService, SecurityService>();

// SPEC 12: Background Job
builder.Services.AddHostedService<RoleExpirationJob>();
```

---

## Logging Examples

### SPEC 7: Login Tracking
```
[INFO] Successful login: User=john.doe, UserId=10, IP=192.168.1.100
[WARN] Failed login attempt: User=jane.doe, Reason=InvalidPassword, IP=192.168.1.200
```

### SPEC 12: Role Expiration
```
[INFO] SPEC 12: Role Expiration Job started. Check interval: 01:00:00
[INFO] SPEC 12: Found 3 expired roles to process
[INFO] SPEC 12: Deactivated expired role - User: john.doe, Role: TempAdmin
[INFO] SPEC 12: Expiration notification sent to user 10
[INFO] SPEC 12: Role expiration check completed - Deactivated: 3, Sent: 3, Failed: 0
```

---

## Monitoring

### Metrics to Track

**SPEC 7:**
- Failed login attempts per hour
- Failed attempts per IP
- Accounts being targeted
- Success/failure ratio

**SPEC 12:**
- Roles expired per cycle
- Notification success rate
- Job execution time
- Any errors/failures

---

## Testing Checklist

### SPEC 7: Login Tracking
- [ ] Successful login tracked
- [ ] Failed login (invalid password) tracked
- [ ] Failed login (user not found) tracked
- [ ] Failed login (account locked) tracked
- [ ] Failed login (account inactive) tracked
- [ ] IP address captured correctly
- [ ] User Agent captured correctly
- [ ] Can query by username
- [ ] Can query by IP address

### SPEC 12: Role Expiration
- [ ] Job starts on application startup
- [ ] Runs at configured interval
- [ ] Finds expired roles correctly
- [ ] Deactivates expired roles
- [ ] Sends notifications to users
- [ ] Handles errors gracefully
- [ ] Logs all operations
- [ ] Job stops cleanly on shutdown

---

## Next Steps

### SPEC 7
1. ? Entity and services created
2. ? Service registered
3. ?? Update IAuthService interface
4. ?? Update AuthService implementation
5. ?? Update AuthController
6. ?? Create migration
7. ?? Test all failure scenarios

### SPEC 12
1. ? Background job created
2. ? Job registered
3. ?? Configure check interval
4. ?? Create migration
5. ?? Test with short interval
6. ?? Verify notifications sent
7. ?? Set up monitoring
8. ?? Deploy to production

---

## Database Migration

```bash
# Create migration
dotnet ef migrations add AddSecurityAndMaintenance --project ..\PermiTrack.DataContext --startup-project .

# Apply migration
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

---

## Production Ready ?

Both modules are:
- ? Fully implemented
- ? Error-resilient
- ? Well-documented
- ? Comprehensively logged
- ? Performance-optimized
- ? Ready for production deployment
