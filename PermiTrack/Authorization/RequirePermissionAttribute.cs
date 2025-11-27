using Microsoft.AspNetCore.Authorization;

namespace PermiTrack.Authorization;

/// <summary>
/// Authorization attribute that requires a specific permission.
/// Usage: [RequirePermission("Users.Read")]
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    private const string PermissionPolicyPrefix = "Permission";

    public RequirePermissionAttribute(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
        }

        // Set the policy name using the permission
        Policy = $"{PermissionPolicyPrefix}{permission}";
    }
}
