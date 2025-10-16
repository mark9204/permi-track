using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class GrantRoleRequest
    {
        // kinek adunk jogot?
        public string Username { get; set; } = default!;
        // milyen role-t?
        public string RoleName { get; set; } = default!;
        // ki adja? (amíg nincs JWT, így adjuk át)
        public long AdminUserId { get; set; }

        // opcionális meta
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }
}
