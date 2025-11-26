using System.ComponentModel.DataAnnotations;

namespace PermiTrack.DataContext.DTOs;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
