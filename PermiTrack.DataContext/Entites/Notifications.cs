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
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // INFO, WARNING, ERROR, SUCCESS
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string RelatedResourceType { get; set; }
        public long? RelatedResourceId { get; set; }

        
        public User User { get; set; }
    }
}
