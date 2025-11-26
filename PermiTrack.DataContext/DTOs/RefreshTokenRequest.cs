using System.ComponentModel.DataAnnotations;

namespace PermiTrack.DataContext.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
