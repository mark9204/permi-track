using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class RolePermission
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public long? GrantedBy { get; set; }

        // Navigation properties
        public Role Role { get; set; } = default!;
        public Permission Permission { get; set; } = default!;
        public User? GrantedByUser { get; set; } // Nullable - audit field
    }
}
