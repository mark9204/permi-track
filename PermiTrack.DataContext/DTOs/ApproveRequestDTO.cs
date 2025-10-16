using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class ApproveRequestDTO
    {

        public long ApproverUserId { get; set; }       // ki hagyja jóvá
        public string? Comment { get; set; }

    }
}
