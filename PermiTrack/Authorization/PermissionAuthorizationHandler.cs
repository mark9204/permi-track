using Microsoft.AspNetCore.Authorization;

namespace PermiTrack.Authorization;

/// <summary>
/// Authorization handler that validates if the user has the required permission claim.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if the user has any permission claims that match the required permission
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
