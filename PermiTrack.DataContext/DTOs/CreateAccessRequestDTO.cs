using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    public class CreateAccessRequestDTO
    {
        public string Username { get; set; } = default!; //-kinek lesz a role
        public string RoleName { get; set; } = default!; // milyen riole

        public long WorkflowId { get; set; } // melyik workflow pl defaultrolegrant --> ezt még kifejtem majd azért hátja

        public DateTime? ExpiresAt { get; set; } // optional ez is

        public string Reason { get; set; } = default!; 

        public long RequestedByUserId { get; set; } // amig nincs authentikácó addig ez jó lesz arra hogy ki inditotta
    }
}
