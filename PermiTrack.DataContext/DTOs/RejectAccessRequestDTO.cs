namespace PermiTrack.DataContext.DTOs;

/// <summary>
/// Request DTO for rejecting an access request
/// </summary>
public class RejectAccessRequestDTO
{
    /// <summary>
    /// Comment explaining why the request was rejected
    /// </summary>
    public string ReviewerComment { get; set; } = string.Empty;
}
