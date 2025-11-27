# Audit Logging System Documentation

## Overview

The Audit Logging System is a comprehensive middleware-based solution that automatically logs all HTTP requests and responses to the database. This provides complete visibility into API usage, security events, and performance metrics.

## Architecture

### Components

1. **HttpAuditLog Entity** - Database entity for storing audit log entries
2. **IAuditService** - Service interface for logging operations
3. **AuditService** - Service implementation with safe error handling
4. **AuditLoggingMiddleware** - Middleware that intercepts all HTTP requests
5. **HttpAuditLogsController** - API endpoints for querying audit logs

## Database Schema

### HttpAuditLog Table

| Column | Type | Description |
|--------|------|-------------|
| Id | bigint | Primary key |
| UserId | bigint? | User ID if authenticated, null for anonymous |
| Username | string? | Username for quick reference |
| Method | string | HTTP method (GET, POST, PUT, DELETE, etc.) |
| Path | string | Request path (e.g., /api/users) |
| QueryString | string? | Query string parameters |
| StatusCode | int | HTTP status code (200, 401, 403, 500, etc.) |
| IpAddress | string | Client IP address |
| UserAgent | string | Browser/client user agent |
| DurationMs | long | Request duration in milliseconds |
| Timestamp | DateTime | When the request was made (UTC) |
| AdditionalInfo | string? | Extra context or error information |

### Indexes

- Primary key on `Id`
- Index on `Timestamp` (for time-based queries)
- Index on `UserId` (for user activity tracking)
- Index on `StatusCode` (for error analysis)
- Composite index on `Method, Path` (for endpoint analysis)

## How It Works

### Request Flow

```
1. HTTP Request arrives
   ?
2. Authentication/Authorization middleware processes request
   ?
3. AuditLoggingMiddleware starts:
   - Starts Stopwatch
   - Captures request details
   ?
4. Request proceeds to Controller
   ?
5. Controller processes and returns response
   ?
6. AuditLoggingMiddleware completes:
   - Stops Stopwatch
   - Extracts user information from context.User
   - Extracts IP address and User Agent
   - Creates HttpAuditLog entry
   - Saves to database asynchronously (fire-and-forget)
   ?
7. Response sent to client
```

### Key Features

1. **Non-Blocking**: Logging happens asynchronously and doesn't block the response
2. **Error Resilient**: If logging fails, the API response is not affected
3. **User Context**: Captures authenticated user information from JWT claims
4. **Performance Tracking**: Measures request duration in milliseconds
5. **IP Address Detection**: Handles proxied requests (X-Forwarded-For, X-Real-IP)
6. **DbContext Safety**: Uses DbContextFactory to avoid lifetime conflicts

## Configuration

### 1. Entity Registration (PermiTrackDbContext.cs)

```csharp
public DbSet<HttpAuditLog> HttpAuditLogs => Set<HttpAuditLog>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<HttpAuditLog>(entity =>
    {
        entity.HasKey(h => h.Id);
        entity.Property(h => h.Method).HasMaxLength(10).IsRequired();
        entity.Property(h => h.Path).HasMaxLength(500).IsRequired();
        // ... more configuration
    });
}
```

### 2. Service Registration (Program.cs)

```csharp
// Add DbContextFactory for Audit Service
builder.Services.AddDbContextFactory<PermiTrackDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();
```

### 3. Middleware Registration (Program.cs)

**IMPORTANT**: The middleware must be placed in the correct order:

```csharp
// Authentication and Authorization MUST come first
app.UseAuthentication();
app.UseAuthorization();

// Audit Logging comes AFTER authentication/authorization
// but BEFORE endpoint mapping
app.UseAuditLogging();

app.MapControllers();
```

**Why this order matters:**
- Placed **AFTER** authentication/authorization: So user context is available
- Placed **BEFORE** endpoint mapping: To capture all requests including 404s

## API Endpoints

All endpoints require the `AuditLogs.Read` permission.

### 1. Get Paginated Audit Logs

```http
GET /api/httpauditlogs?page=1&pageSize=50&username=john&method=GET&path=/api/users&statusCode=200&fromDate=2024-01-01&toDate=2024-12-31
```

**Query Parameters:**
- `page` (default: 1) - Page number
- `pageSize` (default: 50, max: 1000) - Items per page
- `username` - Filter by username (partial match)
- `method` - Filter by HTTP method (GET, POST, etc.)
- `path` - Filter by path (partial match)
- `statusCode` - Filter by status code
- `fromDate` - Filter from date (UTC)
- `toDate` - Filter to date (UTC)

**Response:**
```json
{
  "page": 1,
  "pageSize": 50,
  "totalCount": 1250,
  "totalPages": 25,
  "data": [
    {
      "id": 12345,
      "userId": 42,
      "username": "john.doe",
      "method": "GET",
      "path": "/api/users",
      "queryString": "?page=1",
      "statusCode": 200,
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0...",
      "durationMs": 145,
      "timestamp": "2024-01-15T10:30:00Z",
      "additionalInfo": null
    }
  ]
}
```

### 2. Get Specific Audit Log

```http
GET /api/httpauditlogs/{id}
```

**Response:**
```json
{
  "id": 12345,
  "userId": 42,
  "username": "john.doe",
  "user": {
    "id": 42,
    "username": "john.doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "method": "GET",
  "path": "/api/users",
  "queryString": "?page=1",
  "statusCode": 200,
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "durationMs": 145,
  "timestamp": "2024-01-15T10:30:00Z",
  "additionalInfo": null
}
```

### 3. Get Statistics

```http
GET /api/httpauditlogs/statistics?fromDate=2024-01-01&toDate=2024-12-31
```

**Response:**
```json
{
  "totalRequests": 125000,
  "uniqueUsers": 450,
  "averageDurationMs": 234.56,
  "statusCodeDistribution": [
    { "statusCode": 200, "count": 98000 },
    { "statusCode": 401, "count": 5000 },
    { "statusCode": 403, "count": 2000 },
    { "statusCode": 404, "count": 15000 },
    { "statusCode": 500, "count": 5000 }
  ],
  "methodDistribution": [
    { "method": "GET", "count": 80000 },
    { "method": "POST", "count": 30000 },
    { "method": "PUT", "count": 10000 },
    { "method": "DELETE", "count": 5000 }
  ],
  "topPaths": [
    { "path": "/api/users", "count": 35000 },
    { "path": "/api/roles", "count": 20000 },
    { "path": "/api/permissions", "count": 15000 }
  ]
}
```

### 4. Get User Activity

```http
GET /api/httpauditlogs/user/{userId}?page=1&pageSize=50
```

**Response:**
```json
{
  "userId": 42,
  "page": 1,
  "pageSize": 50,
  "totalCount": 500,
  "totalPages": 10,
  "data": [
    {
      "id": 12345,
      "method": "GET",
      "path": "/api/users",
      "queryString": null,
      "statusCode": 200,
      "ipAddress": "192.168.1.100",
      "durationMs": 145,
      "timestamp": "2024-01-15T10:30:00Z"
    }
  ]
}
```

### 5. Get Failed Authentication Attempts

```http
GET /api/httpauditlogs/security/failed-attempts?page=1&pageSize=50&fromDate=2024-01-01
```

Retrieves all 401 and 403 responses for security monitoring.

**Response:**
```json
{
  "page": 1,
  "pageSize": 50,
  "totalCount": 150,
  "totalPages": 3,
  "data": [
    {
      "id": 12346,
      "userId": null,
      "username": null,
      "method": "POST",
      "path": "/api/auth/login",
      "statusCode": 401,
      "ipAddress": "192.168.1.200",
      "timestamp": "2024-01-15T10:35:00Z"
    }
  ]
}
```

## Use Cases

### 1. Security Monitoring

**Track Failed Login Attempts:**
```http
GET /api/httpauditlogs/security/failed-attempts?fromDate=2024-01-15
```

**Monitor Suspicious Activity:**
```http
GET /api/httpauditlogs?statusCode=403&fromDate=2024-01-15
```

### 2. User Activity Tracking

**See what a specific user has been doing:**
```http
GET /api/httpauditlogs/user/42
```

### 3. Performance Analysis

**Find slow endpoints:**
```http
GET /api/httpauditlogs/statistics
```

Look at `averageDurationMs` and analyze `topPaths`.

### 4. Error Analysis

**Find all server errors:**
```http
GET /api/httpauditlogs?statusCode=500&fromDate=2024-01-15
```

### 5. API Usage Analytics

**Most popular endpoints:**
```http
GET /api/httpauditlogs/statistics
```

Check `topPaths` and `methodDistribution`.

## Error Handling

The system is designed to never disrupt API responses:

1. **Try-Catch in Middleware**: Audit logging is wrapped in try-catch
2. **Safe Logging Method**: `LogSafeAsync()` swallows exceptions
3. **Fire-and-Forget**: Logging happens in `Task.Run()` to avoid blocking
4. **Separate DbContext**: Uses `DbContextFactory` to avoid conflicts

### Error Logging

If audit logging fails, errors are logged to the application logs:

```
Failed to save audit log for GET /api/users. StatusCode: 200
Exception: ...
```

## Performance Considerations

### Database Impact

- **Writes**: One INSERT per request
- **Indexes**: Optimized for common query patterns
- **Async**: Non-blocking writes

### Optimization Tips

1. **Archive Old Logs**: Regularly move old logs to archive tables
2. **Partition Table**: Consider table partitioning for large datasets
3. **Selective Logging**: Optionally exclude certain paths (health checks, etc.)

### Example: Exclude Health Check Endpoints

Modify `AuditLoggingMiddleware.InvokeAsync`:

```csharp
public async Task InvokeAsync(HttpContext context, IAuditService auditService)
{
    // Skip audit logging for health check endpoints
    if (context.Request.Path.StartsWithSegments("/health") ||
        context.Request.Path.StartsWithSegments("/api/health"))
    {
        await _next(context);
        return;
    }

    // ... rest of the code
}
```

## Migration

To create the database table, run:

```bash
dotnet ef migrations add AddHttpAuditLog --project PermiTrack.DataContext --startup-project PermiTrack
dotnet ef database update --project PermiTrack.DataContext --startup-project PermiTrack
```

## Troubleshooting

### Issue: Audit logs not being created

**Check:**
1. DbContextFactory is registered
2. AuditService is registered
3. Middleware is added in correct order
4. Database migration has been run

### Issue: UserId is always null

**Check:**
1. Middleware is placed AFTER `UseAuthentication()`
2. JWT token is valid and contains `ClaimTypes.NameIdentifier`
3. User is actually authenticated

### Issue: Performance degradation

**Check:**
1. Database indexes are created
2. Consider archiving old logs
3. Monitor database write performance
4. Check if logging is truly async (not blocking)

## Security Considerations

1. **PII Data**: Audit logs may contain sensitive information (IP addresses, user data)
2. **Access Control**: Restrict access to audit logs (use `[RequirePermission("AuditLogs.Read")]`)
3. **Retention Policy**: Define how long to keep audit logs
4. **Data Privacy**: Consider GDPR/privacy requirements
5. **Tamper-Proof**: Consider making the table append-only (no UPDATE/DELETE permissions)

## Best Practices

1. **Regular Review**: Regularly review audit logs for security incidents
2. **Automated Alerts**: Set up alerts for suspicious patterns (e.g., many 401s from same IP)
3. **Retention Policy**: Archive or delete old logs based on policy
4. **Monitoring**: Monitor audit log write failures
5. **Privacy**: Be mindful of PII in query strings and additional info

## Future Enhancements

Potential improvements:

1. **Request/Response Body Logging**: Optionally log request/response bodies
2. **Correlation IDs**: Add correlation IDs for request tracking
3. **Geo-Location**: Map IP addresses to geographic locations
4. **Alerting**: Real-time alerts for suspicious activity
5. **Data Retention**: Automatic archival and cleanup jobs
6. **Export**: Export audit logs in various formats (CSV, JSON)
7. **Dashboard**: Visual dashboard for audit log analytics
