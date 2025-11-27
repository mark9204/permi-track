using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites;

public class Permission
{
    public long Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Resource { get; set; } = string.Empty; // e.g. "users", "roles"
    public string Action { get; set; } = string.Empty;   // e.g. "create", "read", "update", "delete"
    
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
