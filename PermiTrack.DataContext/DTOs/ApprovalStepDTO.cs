using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    internal class ApprovalStepDTO
    {
        public long Id {  get; set; }
        public long WorkflowId { get; set; }

        public int StepOrder { get; set; }

        public long ApproverRoleId { get; set; }

        public int RequiredApprovals {  get; set; }

        public bool IsParallel {  get; set; }

    }
}
