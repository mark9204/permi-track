# Audit Logging Quick Reference

## Quick Setup

### 1. Entities Already Created ?
- `HttpAuditLog.cs` - Database entity

### 2. Service Already Created ?
- `IAuditService.cs` - Interface
- `AuditService.cs` - Implementation

### 3. Middleware Already Created ?
- `AuditLoggingMiddleware.cs` - Intercepts requests

### 4. Already Registered in Program.cs ?

```csharp
// DbContextFactory for audit service
builder.Services.AddDbContextFactory<PermiTrackDbContext>(options =>
    options.UseSqlServer(connectionString));

// Audit service
builder.Services.AddScoped<IAuditService, AuditService>();

// Middleware (correct order!)
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditLogging();  // AFTER auth, BEFORE endpoints
app.MapControllers();
```

## Next Steps

### Create Migration

```bash
# From PermiTrack directory
dotnet ef migrations add AddHttpAuditLog --project ..\PermiTrack.DataContext --startup-project .

# Apply migration
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

### Test the System

1. **Start the application**
2. **Make any API request** (e.g., GET /api/users)
3. **Check audit logs**:
   ```http
   GET /api/httpauditlogs?page=1&pageSize=10
   ```

## Common Queries

### View Recent Logs
```http
GET /api/httpauditlogs?page=1&pageSize=50
```

### View Failed Authentications
```http
GET /api/httpauditlogs/security/failed-attempts
```

### View User Activity
```http
GET /api/httpauditlogs/user/{userId}
```

### View Statistics
```http
GET /api/httpauditlogs/statistics
```

### Filter by Method
```http
GET /api/httpauditlogs?method=POST
```

### Filter by Status Code
```http
GET /api/httpauditlogs?statusCode=500
```

### Filter by Date Range
```http
GET /api/httpauditlogs?fromDate=2024-01-01&toDate=2024-12-31
```

## What Gets Logged

Every HTTP request logs:
- ? User ID (if authenticated)
- ? Username (if authenticated)
- ? HTTP Method (GET, POST, etc.)
- ? Request Path
- ? Query String
- ? Status Code
- ? IP Address
- ? User Agent
- ? Duration (milliseconds)
- ? Timestamp

## Key Features

### ? Non-Blocking
Logging happens asynchronously - doesn't slow down API responses

### ? Error Resilient
If logging fails, API still returns the response normally

### ? User Context Aware
Captures authenticated user information from JWT

### ? Performance Tracking
Measures how long each request takes

### ? Proxy-Aware
Handles X-Forwarded-For and X-Real-IP headers

## Architecture

```
Request ? Authentication ? Authorization ? AuditMiddleware ? Controller
                                              ?
                                          (captures)
                                              ?
Response ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?
                                              ?
                                      (logs async)
                                              ?
                                          Database
```

## File Structure

```
PermiTrack/
??? Middleware/
?   ??? AuditLoggingMiddleware.cs
?   ??? AUDIT_LOGGING_DOCUMENTATION.md
??? Extensions/
?   ??? AuditLoggingMiddlewareExtensions.cs
??? Controllers/
?   ??? HttpAuditLogsController.cs
??? Program.cs

PermiTrack.DataContext/
??? Entites/
    ??? HttpAuditLog.cs

PermiTrack.Services/
??? Interfaces/
?   ??? IAuditService.cs
??? Services/
    ??? AuditService.cs
```

## Middleware Order (Critical!)

```csharp
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();      // 1. FIRST - Authenticate user
app.UseAuthorization();       // 2. SECOND - Authorize user
app.UseAuditLogging();        // 3. THIRD - Log with user context
app.MapControllers();         // 4. LAST - Route to controllers
```

## Database Schema

```sql
CREATE TABLE HttpAuditLogs (
    Id BIGINT PRIMARY KEY IDENTITY,
    UserId BIGINT NULL,
    Username NVARCHAR(100) NULL,
    Method NVARCHAR(10) NOT NULL,
    Path NVARCHAR(500) NOT NULL,
    QueryString NVARCHAR(2000) NULL,
    StatusCode INT NOT NULL,
    IpAddress NVARCHAR(50) NOT NULL,
    UserAgent NVARCHAR(500) NULL,
    DurationMs BIGINT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AdditionalInfo NVARCHAR(2000) NULL,
    
    -- Indexes
    INDEX IX_HttpAuditLogs_Timestamp (Timestamp),
    INDEX IX_HttpAuditLogs_UserId (UserId),
    INDEX IX_HttpAuditLogs_StatusCode (StatusCode),
    INDEX IX_HttpAuditLogs_Method_Path (Method, Path)
);
```

## Permissions Required

All HttpAuditLogs endpoints require:
```csharp
[RequirePermission("AuditLogs.Read")]
```

Make sure to:
1. Create the permission in the database
2. Assign it to appropriate roles
3. Grant those roles to users who need to view logs

## Example: Creating the Permission

```sql
-- Insert the permission
INSERT INTO Permissions (Name, Resource, Action, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('AuditLogs.Read', 'AuditLogs', 'Read', 'View HTTP audit logs', 1, GETUTCDATE(), GETUTCDATE());

-- Get the permission ID
DECLARE @PermissionId BIGINT = SCOPE_IDENTITY();

-- Assign to Admin role (assuming role ID 1 is Admin)
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt, GrantedBy)
VALUES (1, @PermissionId, GETUTCDATE(), NULL);
```

## Testing Checklist

- [ ] Create and apply migration
- [ ] Create AuditLogs.Read permission
- [ ] Assign permission to your role
- [ ] Start application
- [ ] Make test request
- [ ] Check logs: `GET /api/httpauditlogs`
- [ ] Verify user info is captured
- [ ] Verify IP address is captured
- [ ] Verify duration is reasonable
- [ ] Test filtering by status code
- [ ] Test statistics endpoint

## Troubleshooting

### No logs appearing?
1. Check migration applied
2. Check middleware is registered
3. Check AuditService is registered
4. Check DbContextFactory is registered
5. Check application logs for errors

### UserId is null?
1. Check middleware order (after authentication)
2. Check JWT contains NameIdentifier claim
3. Check user is actually authenticated

### Permission denied on logs endpoint?
1. Check you have AuditLogs.Read permission
2. Check permission is assigned to your role
3. Check you're authenticated with valid token

## Performance Tips

### For High Traffic
1. Archive old logs regularly
2. Consider table partitioning
3. Monitor database write performance
4. Consider excluding health check endpoints

### Example: Skip Health Checks
```csharp
// In AuditLoggingMiddleware.InvokeAsync
if (context.Request.Path.StartsWithSegments("/health"))
{
    await _next(context);
    return;
}
```

## Monitoring

Watch for:
- ? Audit log write failures (check application logs)
- ? Slow database writes (> 100ms)
- ? Large table size (set retention policy)
- ? Successful audit log creation rate
- ? Failed authentication patterns

## Common Use Cases

### Security Investigation
```http
GET /api/httpauditlogs/security/failed-attempts?fromDate=2024-01-15
```

### User Activity Review
```http
GET /api/httpauditlogs/user/42
```

### Performance Analysis
```http
GET /api/httpauditlogs/statistics
```

### Error Tracking
```http
GET /api/httpauditlogs?statusCode=500&fromDate=2024-01-15
```

### API Usage Analytics
```http
GET /api/httpauditlogs/statistics
```
