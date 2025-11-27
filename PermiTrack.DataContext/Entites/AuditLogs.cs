using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites;

public class AuditLog
{
    public long Id { get; set; }
    public long? UserId { get; set; } // Nullable for system actions
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public long ResourceId { get; set; }
    public string OldValues { get; set; } = string.Empty; // JSON
    public string NewValues { get; set; } = string.Empty; // JSON
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
