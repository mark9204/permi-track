using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // INFO, WARNING, ERROR, SUCCESS
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string RelatedResourceType { get; set; } = string.Empty;
        public long? RelatedResourceId { get; set; }

        // Navigation property
        public User User { get; set; } = default!;
    }
}
