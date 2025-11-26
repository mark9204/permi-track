namespace PermiTrack.DataContext.DTOs;

public class UserActivationRequest
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}
