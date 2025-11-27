# Quick Reference: Permission-Based Authorization

## Quick Start

### 1. Apply Permission to an Endpoint

```csharp
[HttpGet("users")]
[RequirePermission("Users.Read")]
public IActionResult GetUsers()
{
    return Ok(/* data */);
}
```

### 2. Check User Permissions in Code

```csharp
// In your controller or service
var hasPermission = User.Claims.Any(c => 
    c.Type == "permission" && 
    c.Value == "Users.Read");

if (hasPermission)
{
    // User has permission
}
```

### 3. Get All User Permissions

```csharp
var permissions = User.Claims
    .Where(c => c.Type == "permission")
    .Select(c => c.Value)
    .ToList();
```

## Common Permission Patterns

### CRUD Operations
```csharp
[RequirePermission("Resource.Read")]    // GET
[RequirePermission("Resource.Write")]   // POST/PUT
[RequirePermission("Resource.Delete")]  // DELETE
```

### Administrative Actions
```csharp
[RequirePermission("Resource.Manage")]  // Full CRUD
[RequirePermission("Resource.Admin")]   // All operations including configuration
```

## File Structure

```
PermiTrack/
??? Authorization/
?   ??? PermissionRequirement.cs
?   ??? PermissionAuthorizationHandler.cs
?   ??? PermissionPolicyProvider.cs
?   ??? RequirePermissionAttribute.cs
?   ??? README.md
??? Controllers/
?   ??? ExamplePermissionController.cs
??? Program.cs (registration)
```

## Registration Summary (Program.cs)

```csharp
// 1. Add Authorization
builder.Services.AddAuthorization();

// 2. Register Handler (Scoped)
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// 3. Register Policy Provider (Singleton)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
```

## Testing Checklist

- [ ] User can access endpoints with required permission
- [ ] User gets 403 for endpoints without required permission
- [ ] User gets 401 without JWT token
- [ ] Multiple permissions work correctly
- [ ] Controller-level permissions apply to all actions
- [ ] Mixed role + permission authorization works

## Common Issues & Solutions

### Issue: Always getting 403
**Solution:** Check that:
1. Permission claim type is exactly "permission" (case-sensitive in claim type)
2. Permission value matches exactly (case-insensitive in value comparison)
3. User's JWT actually contains the permission claims

### Issue: Permissions not in JWT
**Solution:** Verify:
1. User has roles assigned
2. Roles have permissions assigned
3. `AuthService.GetUserPermissionsAsync()` is called
4. `TokenService.GenerateAccessToken()` receives permissions

### Issue: Custom policy provider not working
**Solution:** Ensure:
1. Policy provider is registered as Singleton
2. Handler is registered (Scoped or Transient)
3. Policy name starts with "Permission" prefix

## Example: Protecting a Complete Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using PermiTrack.Authorization;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [RequirePermission("Products.Read")]
    public IActionResult GetAll()
    {
        return Ok(/* products */);
    }

    [HttpGet("{id}")]
    [RequirePermission("Products.Read")]
    public IActionResult GetById(int id)
    {
        return Ok(/* product */);
    }

    [HttpPost]
    [RequirePermission("Products.Write")]
    public IActionResult Create([FromBody] object data)
    {
        return Ok(/* created */);
    }

    [HttpPut("{id}")]
    [RequirePermission("Products.Write")]
    public IActionResult Update(int id, [FromBody] object data)
    {
        return Ok(/* updated */);
    }

    [HttpDelete("{id}")]
    [RequirePermission("Products.Delete")]
    public IActionResult Delete(int id)
    {
        return NoContent();
    }
}
```

## Next Steps

1. Define your permission naming convention
2. Create permissions in the database
3. Assign permissions to roles
4. Apply `[RequirePermission]` attributes to controllers/actions
5. Test with different user roles
6. Document permissions in API documentation
