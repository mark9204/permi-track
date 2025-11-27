using PermiTrack.DataContext.Enums;
using System;

namespace PermiTrack.DataContext.Entites
{
    /// <summary>
    /// Represents a request for access to a role or permissions
    /// </summary>
    public class AccessRequest
    {
        public long Id { get; set; }

        /// <summary>
        /// ID of the user requesting access
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// ID of the role being requested
        /// </summary>
        public long RequestedRoleId { get; set; }

        /// <summary>
        /// JSON string of specific permissions requested (if applicable)
        /// </summary>
        public string RequestedPermissions { get; set; } = string.Empty;

        /// <summary>
        /// Reason for the access request
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the request
        /// </summary>
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        /// <summary>
        /// When the request was created
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the request was approved (if approved)
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// User ID who approved the request
        /// </summary>
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// When the request was rejected (if rejected)
        /// </summary>
        public DateTime? RejectedAt { get; set; }

        /// <summary>
        /// User ID who rejected the request
        /// </summary>
        public long? RejectedBy { get; set; }

        /// <summary>
        /// When the granted access should expire (null = no expiration)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Requested duration in hours (used to calculate ExpiresAt when approved)
        /// </summary>
        public int? RequestedDurationHours { get; set; }

        /// <summary>
        /// Comment from the reviewer (approver or rejecter)
        /// </summary>
        public string? ReviewerComment { get; set; }

        /// <summary>
        /// User ID who processed (approved/rejected) the request
        /// </summary>
        public long? ProcessedByUserId { get; set; }

        /// <summary>
        /// When the request was processed (approved/rejected)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID of the approval workflow (if using workflow system)
        /// </summary>
        public long? WorkflowId { get; set; }

        /// <summary>
        /// Current step in the workflow (if using multi-step approval)
        /// </summary>
        public long? CurrentStepId { get; set; }

        // Navigation properties
        public User User { get; set; } = default!;
        public Role RequestedRole { get; set; } = default!;
        public User? ApprovedByUser { get; set; }
        public User? RejectedByUser { get; set; }
        public User? ProcessedByUser { get; set; }
        public ApprovalWorkflow? Workflow { get; set; }
        public ApprovalStep? CurrentStep { get; set; }
    }
}
