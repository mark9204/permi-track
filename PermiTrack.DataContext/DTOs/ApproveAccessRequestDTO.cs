namespace PermiTrack.DataContext.DTOs;

/// <summary>
/// Request DTO for approving an access request
/// </summary>
public class ApproveAccessRequestDTO
{
    /// <summary>
    /// Optional comment from the approver
    /// </summary>
    public string? ReviewerComment { get; set; }

    /// <summary>
    /// Override the requested duration (in hours)
    /// If null, uses the originally requested duration
    /// </summary>
    public int? OverrideDurationHours { get; set; }
}
