using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    internal class AccessRequestDTO
    {

        public long Id { get; set; }

        public long UserId { get; set; }

        public long RequestedRoleId { get; set; }

        public string RequestedPermissions { get; set; } = default!;

        public string Reason { get; set; } = default!;

        public string Status { get; set; } = default!;

        public DateTime RequestedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

    }
}
