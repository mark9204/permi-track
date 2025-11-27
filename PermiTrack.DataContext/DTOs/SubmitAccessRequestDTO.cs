namespace PermiTrack.DataContext.DTOs;

/// <summary>
/// Request DTO for submitting an access request
/// </summary>
public class SubmitAccessRequestDTO
{
    /// <summary>
    /// ID of the role being requested
    /// </summary>
    public long RequestedRoleId { get; set; }

    /// <summary>
    /// Reason for requesting this role
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Requested duration in hours (optional, null = permanent)
    /// </summary>
    public int? RequestedDurationHours { get; set; }

    /// <summary>
    /// Specific permissions being requested (optional, JSON string)
    /// </summary>
    public string? RequestedPermissions { get; set; }
}
