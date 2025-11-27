using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites;

public class Role
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int Level { get; set; } = 0;

    // ROLE HIERARCHY: Self-referencing relationship
    public long? ParentRoleId { get; set; }
    public Role? ParentRole { get; set; }
    public ICollection<Role> SubRoles { get; set; } = new List<Role>();

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<ApprovalStep> ApprovalSteps { get; set; }
}
