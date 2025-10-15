using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class RoleDTO
    {
        public long Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int Level { get; set; }

    }
}
