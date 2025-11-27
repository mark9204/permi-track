using Microsoft.AspNetCore.Authorization;

namespace PermiTrack.Authorization;

/// <summary>
/// Represents an authorization requirement that checks for a specific permission claim.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
        }

        Permission = permission;
    }
}
