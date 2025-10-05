using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class AuditLog
    {
        public long Id { get; set; }
        public long? UserId { get; set; } // Nullable for system actions
        public string Action { get; set; }
        public string ResourceType { get; set; }
        public long ResourceId { get; set; }
        public string OldValues { get; set; } // JSON
        public string NewValues { get; set; } // JSON
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
