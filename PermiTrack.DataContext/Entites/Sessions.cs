using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class Sessions
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastActivity { get; set; }

        // Navigációs property
        public User User { get; set; }
    }
}
