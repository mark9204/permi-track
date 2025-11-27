using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using PermiTrack.DataContext.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
    /// <summary>
    /// Response DTO for access request details
    /// </summary>
    public class AccessRequestDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public long RequestedRoleId { get; set; }
        public string RequestedRoleName { get; set; } = string.Empty;
        public string RequestedPermissions { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long? ApprovedBy { get; set; }
        public string? ApprovedByUsername { get; set; }
        public DateTime? RejectedAt { get; set; }
        public long? RejectedBy { get; set; }
        public string? RejectedByUsername { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? RequestedDurationHours { get; set; }
        public string? ReviewerComment { get; set; }
        public long? ProcessedByUserId { get; set; }
        public string? ProcessedByUsername { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
