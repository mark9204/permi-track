using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class ApprovalStep
    {
        public long Id { get; set; }
        public long WorkflowId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; } = 0;
        public long ApproverRoleId { get; set; }
        public int RequiredApprovals { get; set; } = 1;
        public bool IsParallel { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApprovalWorkflow Workflow { get; set; } = default!;
        public Role ApproverRole { get; set; } = default!;
    }
}
