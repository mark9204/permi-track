using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class Permission
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
