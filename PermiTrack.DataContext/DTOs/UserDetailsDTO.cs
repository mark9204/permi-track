namespace PermiTrack.DataContext.DTOs;

public class UserDetailsDTO
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public List<UserRoleDTO> Roles { get; set; } = new();
    public List<UserPermissionDTO> DirectPermissions { get; set; } = new();
}

public class UserRoleDTO
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class UserPermissionDTO
{
    public long PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
