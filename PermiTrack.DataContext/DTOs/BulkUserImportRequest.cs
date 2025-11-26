namespace PermiTrack.DataContext.DTOs;

public class BulkUserImportRequest
{
    public List<BulkUserItem> Users { get; set; } = new();
    public bool SendWelcomeEmail { get; set; } = true;
    public bool RequirePasswordChange { get; set; } = true;
}

public class BulkUserItem
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public List<string>? RoleNames { get; set; }
}
