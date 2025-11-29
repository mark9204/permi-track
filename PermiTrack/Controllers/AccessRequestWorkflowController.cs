using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermiTrack.Authorization;
using PermiTrack.DataContext.DTOs;
using PermiTrack.Services.Interfaces;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PermiTrack.Controllers;

/// <summary>
/// Controller for managing access request workflow
/// Handles submission, approval, and rejection of role access requests
/// </summary>
[ApiController]
[Route("api/access-requests")]
[Authorize]
public class AccessRequestWorkflowController : ControllerBase
{
    private readonly IAccessRequestService _accessRequestService;
    private readonly ILogger<AccessRequestWorkflowController> _logger;

    public AccessRequestWorkflowController(
        IAccessRequestService accessRequestService,
        ILogger<AccessRequestWorkflowController> logger)
    {
        _accessRequestService = accessRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new access request for a role
    /// Any authenticated user can submit a request
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitAccessRequestDTO request)
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _accessRequestService.SubmitRequestAsync(userId, request);

            return CreatedAtAction(
                nameof(GetRequestById),
                new { id = result.Id },
                new
                {
                    message = "Access request submitted successfully",
                    request = result
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to submit access request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting access request");
            return StatusCode(500, new { message = "An error occurred while submitting the request" });
        }
    }

    /// <summary>
    /// Approve an access request
    /// Requires AccessRequests.Manage permission
    /// </summary>
    [HttpPut("{id}/approve")]
    //[RequirePermission("AccessRequests.Manage")]
    public async Task<IActionResult> ApproveRequest(
        long id,
        [FromBody] ApproveAccessRequestDTO? approvalDto = null)
    {
        try
        {
            // Get current user ID (reviewer)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var reviewerId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _accessRequestService.ApproveRequestAsync(id, reviewerId, approvalDto);

            return Ok(new
            {
                message = "Access request approved successfully. Role has been granted to the user.",
                request = result
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to approve access request {RequestId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving access request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while approving the request" });
        }
    }

    /// <summary>
    /// Reject an access request
    /// Requires AccessRequests.Manage permission
    /// </summary>
    [HttpPut("{id}/reject")]
    //[RequirePermission("AccessRequests.Manage")]
    public async Task<IActionResult> RejectRequest(
        long id,
        [FromBody] RejectAccessRequestDTO rejectionDto)
    {
        try
        {
            // Get current user ID (reviewer)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var reviewerId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _accessRequestService.RejectRequestAsync(id, reviewerId, rejectionDto);

            return Ok(new
            {
                message = "Access request rejected",
                request = result
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to reject access request {RequestId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting access request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while rejecting the request" });
        }
    }

    /// <summary>
    /// Cancel a pending access request
    /// User can only cancel their own requests
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelRequest(long id)
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _accessRequestService.CancelRequestAsync(id, userId);

            return Ok(new
            {
                message = "Access request cancelled",
                request = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to cancel access request {RequestId}", id);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to cancel access request {RequestId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling access request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while cancelling the request" });
        }
    }

    /// <summary>
    /// Get current user's access requests
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        try
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var requests = await _accessRequestService.GetUserRequestsAsync(userId);

            return Ok(new
            {
                userId,
                totalCount = requests.Count(),
                requests
            });
        }
        catch (SqlException sqlEx)
        {
            // Database schema mismatch or other SQL error
            _logger.LogError(sqlEx, "SQL error retrieving user access requests for user {UserId}", User?.Identity?.Name ?? "unknown");
            var details = sqlEx.Message;
            return StatusCode(503, new
            {
                message = "Temporary service unavailable due to database schema or SQL issue.",
                hint = "Check that EF migrations have been applied (dotnet ef database update) and that the database schema matches the application model.",
                sqlError = details
            });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "EF Core error retrieving user access requests for user {UserId}", User?.Identity?.Name ?? "unknown");
            return StatusCode(503, new
            {
                message = "Temporary service unavailable due to database update/structure issue.",
                hint = "Ensure database migrations are applied.",
                error = dbEx.InnerException?.Message ?? dbEx.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user access requests");
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    
    [HttpGet("pending")]

    //[RequirePermission("AccessRequests.Manage")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var requests = await _accessRequestService.GetPendingRequestsAsync();
            
            return Ok(new
            {
                totalCount = requests.Count(),
                requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending access requests");
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    /// <summary>
    /// Get a specific access request by ID
    /// Users can view their own requests, managers can view all
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequestById(long id)
    {
        try
        {
            var request = await _accessRequestService.GetRequestByIdAsync(id);

            if (request == null)
            {
                return NotFound(new { message = "Access request not found" });
            }

            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Check if user has permission to view this request
            var hasManagePermission = User.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value.Equals("AccessRequests.Manage", StringComparison.OrdinalIgnoreCase));

            if (request.UserId != userId && !hasManagePermission)
            {
                return Forbid();
            }

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the request" });
        }
    }

    /// <summary>
    /// Get all access requests with optional filtering
    /// Requires AccessRequests.Manage permission
    /// </summary>
    [HttpGet]
    //[RequirePermission("AccessRequests.Manage")]
    public async Task<IActionResult> GetAllRequests(
        [FromQuery] string? status = null,
        [FromQuery] long? userId = null,
        [FromQuery] long? roleId = null)
    {
        try
        {
            var requests = await _accessRequestService.GetAllRequestsAsync(status, userId, roleId);

            return Ok(new
            {
                filters = new { status, userId, roleId },
                totalCount = requests.Count(),
                requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access requests");
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    /// <summary>
    /// Get statistics about access requests
    /// Requires AccessRequests.Manage permission
    /// </summary>
    [HttpGet("statistics")]
    //[RequirePermission("AccessRequests.Manage")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var allRequests = await _accessRequestService.GetAllRequestsAsync();

            var stats = new
            {
                total = allRequests.Count(),
                pending = allRequests.Count(r => r.Status == DataContext.Enums.RequestStatus.Pending),
                approved = allRequests.Count(r => r.Status == DataContext.Enums.RequestStatus.Approved),
                rejected = allRequests.Count(r => r.Status == DataContext.Enums.RequestStatus.Rejected),
                cancelled = allRequests.Count(r => r.Status == DataContext.Enums.RequestStatus.Cancelled),
                
                // Average processing time for completed requests
                averageProcessingTimeHours = allRequests
                    .Where(r => r.ProcessedAt.HasValue)
                    .Select(r => (r.ProcessedAt!.Value - r.RequestedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average(),

                // Most requested roles
                topRequestedRoles = allRequests
                    .GroupBy(r => new { r.RequestedRoleId, r.RequestedRoleName })
                    .Select(g => new
                    {
                        roleId = g.Key.RequestedRoleId,
                        roleName = g.Key.RequestedRoleName,
                        count = g.Count()
                    })
                    .OrderByDescending(x => x.count)
                    .Take(5)
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access request statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }
}
