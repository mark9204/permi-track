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
        public string RequestedPermissions { get; set; } = string.Empty; // JSON 
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public long? RejectedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long WorkflowId { get; set; }
        public long? CurrentStepId { get; set; }

        // Navigation properties
        public User User { get; set; } = default!;
        public Role RequestedRole { get; set; } = default!;
        public User? ApprovedByUser { get; set; } // Nullable - audit field
        public User? RejectedByUser { get; set; } // Nullable - audit field
        public ApprovalWorkflow Workflow { get; set; } = default!;
        public ApprovalStep? CurrentStep { get; set; }
    }
}
