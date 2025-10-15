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
        public DateTime GrantedAt { get; set; }
        public long? GrantedBy { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public Permission Permission { get; set; }
        public User GrantedByUser { get; set; }
    }
}
