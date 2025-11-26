namespace PermiTrack.DataContext.DTOs;

public class SessionDTO
{
    public long Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime ExpiresAt { get; set; }
}
