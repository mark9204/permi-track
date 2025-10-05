using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.Entites
{
    public class AccessRequest
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RequestedRoleId { get; set; }
        public string RequestedPermissions { get; set; } // JSON 
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public long? RejectedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long WorkflowId { get; set; }
        public long? CurrentStepId { get; set; }

        // Navigation properties (nullable where appropriate)
        public User User { get; set; }
        public Role RequestedRole { get; set; }
        public User ApprovedByUser { get; set; }
        public User RejectedByUser { get; set; }
        public ApprovalWorkflow Workflow { get; set; }
        public ApprovalStep CurrentStep { get; set; }
    }
}
