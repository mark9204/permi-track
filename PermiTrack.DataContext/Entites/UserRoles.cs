using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class UserRole
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public long AssignedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
        public User AssignedByUser { get; set; }
    }
}
