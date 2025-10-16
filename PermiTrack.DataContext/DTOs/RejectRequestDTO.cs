using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class RejectRequestDTO
    {
        public long ApproverUserId { get; set; }
        public string Reason { get; set; } = default!;
    }
}
