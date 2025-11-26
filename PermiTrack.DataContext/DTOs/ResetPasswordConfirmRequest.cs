using System.ComponentModel.DataAnnotations;

namespace PermiTrack.DataContext.DTOs;

public class ResetPasswordConfirmRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}
