using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using System.Text.Json;

namespace PermiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessRequestsController : ControllerBase
{
    private readonly PermiTrackDbContext _db;
    public AccessRequestsController(PermiTrackDbContext db) => _db = db;

    // 1) Create access request
    [HttpPost]
    public async Task<IActionResult> Create(CreateAccessRequestDTO req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null) return NotFound(new { message = $"User '{req.Username}' not found" });

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == req.RoleName);
        if (role is null) return NotFound(new { message = $"Role '{req.RoleName}' not found" });

        var wf = await _db.ApprovalWorkflows.FindAsync(req.WorkflowId);
        if (wf is null) return NotFound(new { message = $"Workflow {req.WorkflowId} not found" });

        // Get the first step
        var firstStep = await _db.ApprovalSteps
            .Where(s => s.WorkflowId == wf.Id)
            .OrderBy(s => s.StepOrder)
        .FirstOrDefaultAsync();

        var ar = new AccessRequest
        {
            UserId = user.Id,
            RequestedRoleId = role.Id,
            RequestedPermissions = "[]", // Not used currently
            Reason = req.Reason,
            Status = "PENDING",
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = req.ExpiresAt,
            WorkflowId = wf.Id,
            CurrentStepId = firstStep?.Id
        };
        _db.AccessRequests.Add(ar);

        // Create audit log
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = req.RequestedByUserId,
            Action = "ACCESS_REQUEST_CREATED",
            ResourceType = "AccessRequests",
            ResourceId = 0,
            NewValues = JsonSerializer.Serialize(new { ar.UserId, RoleId = ar.RequestedRoleId, ar.WorkflowId, ar.ExpiresAt, ar.Reason }),
            CreatedAt = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        await _db.SaveChangesAsync();

        // Send notification to approver role of the first step
        if (firstStep != null)
        {
            await NotifyApproverRole(ar.Id, firstStep.ApproverRoleId,
                $"New access request #{ar.Id}", $"User: {req.Username}, Role: {req.RoleName}");
        }

        return CreatedAtAction(nameof(Get), new { id = ar.Id }, new { ar.Id, ar.Status, ar.CurrentStepId });
    }

    // 2) Get access request by ID
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var x = await _db.AccessRequests.AsNoTracking()
            .Include(a => a.CurrentStep)
            .FirstOrDefaultAsync(a => a.Id == id);

        return x is null ? NotFound() : Ok(new
        {
            x.Id,
            x.Status,
            x.RequestedAt,
            x.ApprovedAt,
            x.RejectedAt,
            x.ExpiresAt,
            CurrentStep = x.CurrentStep == null ? null : new { x.CurrentStep.Id, x.CurrentStep.StepOrder }
        });
    }

    // 3) Approve current step
    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id, ApproveRequestDTO dto)
    {
        var ar = await _db.AccessRequests.FindAsync(id);
        if (ar is null) return NotFound();
        if (ar.Status != "PENDING") return BadRequest(new { message = "Request is not pending." });

        // Get current step and check approver permissions
        var step = ar.CurrentStepId == null ? null : await _db.ApprovalSteps.FindAsync(ar.CurrentStepId);
        if (step == null) return BadRequest(new { message = "No current step configured." });

        // Check if approver has the required role for this step
        var hasRole = await _db.UserRoles.AnyAsync(ur =>
            ur.UserId == dto.ApproverUserId &&
            ur.RoleId == step.ApproverRoleId &&
            ur.IsActive &&
            (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow));

        if (!hasRole) return Forbid($"User {dto.ApproverUserId} is not allowed to approve step {step.Id}.");

        // Count approvals (Simple MVP: step.RequiredApprovals=1, no separate approvals table)
        // For multiple approvals, consider adding an ApprovalEvents table. Currently MVP: one click completes step.

        // Close current step and move to next step or finalize
        var next = await _db.ApprovalSteps
            .Where(s => s.WorkflowId == step.WorkflowId && s.StepOrder > step.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefaultAsync();

        if (next == null)
        {
            // Final approval - assign the role
            ar.Status = "APPROVED";
            ar.ApprovedAt = DateTime.UtcNow;

            _db.UserRoles.Add(new UserRole
            {
                UserId = ar.UserId,
                RoleId = ar.RequestedRoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = dto.ApproverUserId,
                ExpiresAt = ar.ExpiresAt,
                IsActive = true
            });

            await _db.Notifications.AddAsync(new Notification
            {
                UserId = ar.UserId,
                Title = "Access granted",
                Message = $"Role {ar.RequestedRoleId} has been assigned.",
                Type = "SUCCESS",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedResourceType = "AccessRequests",
                RelatedResourceId = ar.Id
            });

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = dto.ApproverUserId,
                Action = "ACCESS_REQUEST_APPROVED",
                ResourceType = "AccessRequests",
                ResourceId = ar.Id,
                NewValues = JsonSerializer.Serialize(new { ar.Status, ar.ApprovedAt }),
                CreatedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });
        }
        else
        {
            // Move to next step
            ar.CurrentStepId = next.Id;

            await NotifyApproverRole(ar.Id, next.ApproverRoleId,
                $"Approval needed #{ar.Id}", "Approval needed for next step.");

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = dto.ApproverUserId,
                Action = "ACCESS_REQUEST_STEP_APPROVED",
                ResourceType = "AccessRequests",
                ResourceId = ar.Id,
                NewValues = JsonSerializer.Serialize(new { NextStepId = next.Id }),
                CreatedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { ar.Id, ar.Status, ar.ApprovedAt, ar.CurrentStepId });
    }

    // 4) Reject access request
    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, RejectRequestDTO dto)
    {
        var ar = await _db.AccessRequests.FindAsync(id);
        if (ar is null) return NotFound();
        if (ar.Status != "PENDING") return BadRequest(new { message = "Request is not pending." });

        var step = ar.CurrentStepId == null ? null : await _db.ApprovalSteps.FindAsync(ar.CurrentStepId);
        if (step == null) return BadRequest(new { message = "No current step configured." });

        var hasRole = await _db.UserRoles.AnyAsync(ur =>
            ur.UserId == dto.ApproverUserId &&
            ur.RoleId == step.ApproverRoleId &&
            ur.IsActive &&
            (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow));
        if (!hasRole) return Forbid();

        ar.Status = "REJECTED";
        ar.RejectedAt = DateTime.UtcNow;
        ar.RejectedBy = dto.ApproverUserId;

        await _db.Notifications.AddAsync(new Notification
        {
            UserId = ar.UserId,
            Title = "Access denied",
            Message = dto.Reason,
            Type = "ERROR",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedResourceType = "AccessRequests",
            RelatedResourceId = ar.Id
        });

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = dto.ApproverUserId,
            Action = "ACCESS_REQUEST_REJECTED",
            ResourceType = "AccessRequests",
            ResourceId = ar.Id,
            NewValues = JsonSerializer.Serialize(new { Reason = dto.Reason }),
            CreatedAt = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        await _db.SaveChangesAsync();
        return Ok(new { ar.Id, ar.Status, ar.RejectedAt });
    }

    // 5) Get pending requests for approver
    [HttpGet("pending-for/{approverUserId:long}")]
    public async Task<IActionResult> PendingFor(long approverUserId)
    {
        // Get all roles the user has
        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == approverUserId && ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var list = await _db.AccessRequests
            .Where(a => a.Status == "PENDING")
            .Join(_db.ApprovalSteps, a => a.CurrentStepId, s => s.Id, (a, s) => new { a, s })
            .Where(x => roleIds.Contains(x.s.ApproverRoleId))
            .OrderBy(x => x.a.RequestedAt)
            .Select(x => new { x.a.Id, x.a.UserId, x.a.RequestedRoleId, x.a.RequestedAt, Step = x.s.StepOrder })
            .ToListAsync();

        return Ok(list);
    }

    // Helper method: Send notification to all users with the ApproverRole
    private async Task NotifyApproverRole(long accessRequestId, long approverRoleId, string title, string message)
    {
        var approvers = await _db.UserRoles
            .Where(ur => ur.RoleId == approverRoleId && ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var uid in approvers)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = title,
                Message = message,
                Type = "INFO",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedResourceType = "AccessRequests",
                RelatedResourceId = accessRequestId
            });
        }
    }
}
