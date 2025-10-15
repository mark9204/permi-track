using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class CreatePermissionRequest
    {
        public string Name { get; set; } = default!;
        public string Resource { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
