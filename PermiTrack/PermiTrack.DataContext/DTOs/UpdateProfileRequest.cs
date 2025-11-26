using System.ComponentModel.DataAnnotations;

namespace PermiTrack.DataContext.DTOs;

public class UpdateProfileRequest
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }
}
