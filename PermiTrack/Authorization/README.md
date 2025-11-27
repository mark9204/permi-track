# Permission-Based Authorization System

This document explains the Permission-Based Authorization system implemented in the PermiTrack application.

## Overview

The system uses Policy-Based Authorization to validate permissions stored as claims in JWT tokens. When a user logs in, their permissions are retrieved from the database and added to the JWT token as "permission" claims. These claims are then validated by the authorization system when accessing protected endpoints.

## Components

### 1. PermissionRequirement
**Location:** `Authorization/PermissionRequirement.cs`

Implements `IAuthorizationRequirement` and represents a requirement for a specific permission.

```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
```

### 2. PermissionAuthorizationHandler
**Location:** `Authorization/PermissionAuthorizationHandler.cs`

Handles the validation of permission requirements by checking if the user's JWT contains the required permission claim.

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permission" &&
            c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 3. PermissionPolicyProvider
**Location:** `Authorization/PermissionPolicyProvider.cs`

Dynamically creates authorization policies based on permission names. This allows policies to be created on-the-fly without pre-registering them.

```csharp
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    // Creates policies dynamically based on permission names
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Permission", StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        var permission = policyName.Substring("Permission".Length);
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
```

### 4. RequirePermissionAttribute
**Location:** `Authorization/RequirePermissionAttribute.cs`

A convenient attribute for applying permission requirements to controller actions.

```csharp
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission{permission}";
    }
}
```

## Registration in Program.cs

The authorization system is registered in `Program.cs`:

```csharp
// Authorization Configuration with Permission-Based Policies
builder.Services.AddAuthorization();

// Register Permission Authorization Handler (Scoped)
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Register Custom Policy Provider (Singleton)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
```

## Usage Examples

### Basic Usage in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Requires "Users.Read" permission
    [HttpGet]
    [RequirePermission("Users.Read")]
    public IActionResult GetUsers()
    {
        return Ok(/* user data */);
    }

    // Requires "Users.Write" permission
    [HttpPost]
    [RequirePermission("Users.Write")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        return Ok(/* created user */);
    }

    // Requires "Users.Delete" permission
    [HttpDelete("{id}")]
    [RequirePermission("Users.Delete")]
    public IActionResult DeleteUser(int id)
    {
        return Ok(/* result */);
    }
}
```

### Multiple Permissions

You can apply multiple permission requirements to a single endpoint:

```csharp
[HttpPut("{id}")]
[RequirePermission("Users.Write")]
[RequirePermission("Users.Manage")]
public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
{
    // User needs BOTH permissions to access this endpoint
    return Ok(/* updated user */);
}
```

### Controller-Level Permissions

Apply permissions to entire controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
[RequirePermission("Admin.Access")]
public class AdminController : ControllerBase
{
    // All actions require "Admin.Access" permission
    
    [HttpGet("settings")]
    public IActionResult GetSettings() { /* ... */ }
    
    [HttpPost("settings")]
    public IActionResult UpdateSettings() { /* ... */ }
}
```

### Mixing with Other Authorization

You can combine permission-based authorization with role-based:

```csharp
[HttpGet("sensitive-data")]
[Authorize(Roles = "Administrator")]
[RequirePermission("Data.Read")]
public IActionResult GetSensitiveData()
{
    // User must be in "Administrator" role AND have "Data.Read" permission
    return Ok(/* data */);
}
```

## Permission Naming Convention

It's recommended to use a hierarchical naming convention for permissions:

- **Resource.Action** format (e.g., `Users.Read`, `Roles.Write`)
- Common actions: `Read`, `Write`, `Delete`, `Manage`, `Admin`
- Examples:
  - `Users.Read` - Read user data
  - `Users.Write` - Create/Update users
  - `Users.Delete` - Delete users
  - `Roles.Manage` - Manage roles
  - `Permissions.Admin` - Full permission administration
  - `Reports.Generate` - Generate reports
  - `Settings.Modify` - Modify system settings

## How It Works

1. **Login:** User authenticates and the `AuthService` retrieves their permissions from the database
2. **Token Generation:** `TokenService` adds permissions as "permission" claims to the JWT
3. **Request:** User makes a request to a protected endpoint with the JWT in the Authorization header
4. **Authentication:** JWT middleware validates the token and extracts claims
5. **Authorization:** 
   - The `RequirePermission` attribute creates a policy name (e.g., "PermissionUsers.Read")
   - `PermissionPolicyProvider` creates a policy with a `PermissionRequirement`
   - `PermissionAuthorizationHandler` checks if the user has the required permission claim
6. **Access:** If the permission exists, access is granted; otherwise, a 403 Forbidden is returned

## Database Structure

Permissions are stored and managed through:
- `Permissions` table - Defines available permissions
- `RolePermissions` table - Links roles to permissions
- `UserRoles` table - Links users to roles

When a user logs in, their permissions are aggregated from all their roles.

## Testing

To test the permission system:

1. Create a user and assign them roles with specific permissions
2. Login to get a JWT token
3. Make requests to protected endpoints with the token
4. Verify that:
   - Endpoints with required permissions are accessible
   - Endpoints without required permissions return 403 Forbidden

## Security Considerations

- Permissions are validated on every request
- Permission claims are signed and cannot be tampered with (JWT signature validation)
- Case-insensitive permission matching prevents bypass through casing
- Fallback to default policy provider maintains compatibility with standard ASP.NET Core authorization

## Example Response Codes

- **200 OK** - Request successful, user has permission
- **401 Unauthorized** - No valid JWT token provided
- **403 Forbidden** - Valid token but missing required permission
- **404 Not Found** - Endpoint doesn't exist

## Future Enhancements

Potential improvements to consider:

1. Permission hierarchy (e.g., `Users.Admin` implies `Users.Read` and `Users.Write`)
2. Context-aware permissions (e.g., users can only edit their own data)
3. Permission caching for better performance
4. Audit logging of permission checks
5. Dynamic permission loading from configuration
