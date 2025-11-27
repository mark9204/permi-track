using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PermiTrack.DataContext;
using PermiTrack.DataContext.DTOs;
using PermiTrack.DataContext.Entites;
using PermiTrack.DataContext.Enums;
using PermiTrack.Services.Interfaces;

namespace PermiTrack.Services.Services;

/// <summary>
/// Service for managing access request workflow with transaction support
/// </summary>
public class AccessRequestService : IAccessRequestService
{
    private readonly PermiTrackDbContext _context;
    private readonly ILogger<AccessRequestService> _logger;
    private readonly INotificationService _notificationService;

    public AccessRequestService(
        PermiTrackDbContext context,
        ILogger<AccessRequestService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<AccessRequestDTO> SubmitRequestAsync(long userId, SubmitAccessRequestDTO request)
    {
        // Validate user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Validate role exists
        var role = await _context.Roles.FindAsync(request.RequestedRoleId);
        if (role == null)
        {
            throw new InvalidOperationException("Requested role not found");
        }

        // Check if user already has this role
        var existingUserRole = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == request.RequestedRoleId && ur.IsActive);

        if (existingUserRole)
        {
            throw new InvalidOperationException("User already has this role");
        }

        // Check if there's already a pending request for this role
        var pendingRequest = await _context.AccessRequests
            .AnyAsync(ar => ar.UserId == userId && 
                           ar.RequestedRoleId == request.RequestedRoleId && 
                           ar.Status == RequestStatus.Pending);

        if (pendingRequest)
        {
            throw new InvalidOperationException("A pending request for this role already exists");
        }

        // Create new access request
        var accessRequest = new AccessRequest
        {
            UserId = userId,
            RequestedRoleId = request.RequestedRoleId,
            Reason = request.Reason,
            RequestedPermissions = request.RequestedPermissions ?? string.Empty,
            RequestedDurationHours = request.RequestedDurationHours,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Access request {RequestId} submitted by user {UserId} for role {RoleId}",
            accessRequest.Id, userId, request.RequestedRoleId);

        return await MapToDTO(accessRequest);
    }

    public async Task<AccessRequestDTO> ApproveRequestAsync(
        long requestId, 
        long reviewerUserId, 
        ApproveAccessRequestDTO? approvalDto = null)
    {
        // Start a database transaction - CRITICAL for data consistency
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Load the access request with all related entities
            var accessRequest = await _context.AccessRequests
                .Include(ar => ar.User)
                .Include(ar => ar.RequestedRole)
                .FirstOrDefaultAsync(ar => ar.Id == requestId);

            if (accessRequest == null)
            {
                throw new InvalidOperationException("Access request not found");
            }

            // Validate request is in pending status
            if (accessRequest.Status != RequestStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Cannot approve request with status: {accessRequest.Status}. Only pending requests can be approved.");
            }

            // Validate reviewer exists
            var reviewer = await _context.Users.FindAsync(reviewerUserId);
            if (reviewer == null)
            {
                throw new InvalidOperationException("Reviewer user not found");
            }

            // Check if user still doesn't have this role (double-check in case of race conditions)
            var existingUserRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == accessRequest.UserId && 
                               ur.RoleId == accessRequest.RequestedRoleId && 
                               ur.IsActive);

            if (existingUserRole)
            {
                throw new InvalidOperationException("User already has this role");
            }

            // Update access request status
            accessRequest.Status = RequestStatus.Approved;
            accessRequest.ApprovedAt = DateTime.UtcNow;
            accessRequest.ApprovedBy = reviewerUserId;
            accessRequest.ProcessedByUserId = reviewerUserId;
            accessRequest.ProcessedAt = DateTime.UtcNow;
            accessRequest.ReviewerComment = approvalDto?.ReviewerComment;

            // Calculate expiration date if duration is specified
            DateTime? expiresAt = null;
            var durationHours = approvalDto?.OverrideDurationHours ?? accessRequest.RequestedDurationHours;
            
            if (durationHours.HasValue && durationHours.Value > 0)
            {
                expiresAt = DateTime.UtcNow.AddHours(durationHours.Value);
                accessRequest.ExpiresAt = expiresAt;
            }

            // CRITICAL: Actually grant the role to the user
            var userRole = new UserRole
            {
                UserId = accessRequest.UserId,
                RoleId = accessRequest.RequestedRoleId,
                AssignedBy = reviewerUserId,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);

            // Save all changes
            await _context.SaveChangesAsync();

            // Commit transaction - both access request update and role grant succeed together
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Access request {RequestId} approved by user {ReviewerId}. Role {RoleId} granted to user {UserId}",
                requestId, reviewerUserId, accessRequest.RequestedRoleId, accessRequest.UserId);

            // INTEGRATION: Send notification to the requester AFTER successful commit
            try
            {
                var expirationInfo = expiresAt.HasValue 
                    ? $" (expires at {expiresAt.Value:yyyy-MM-dd HH:mm})" 
                    : "";

                await _notificationService.SendNotificationAsync(
                    accessRequest.UserId,
                    "Access Request Approved! ?",
                    $"Your request for the '{accessRequest.RequestedRole.Name}' role has been approved! " +
                    $"You now have access to this role{expirationInfo}.",
                    NotificationType.Success,
                    "AccessRequest",
                    accessRequest.Id);

                _logger.LogInformation(
                    "Approval notification sent to user {UserId} for request {RequestId}",
                    accessRequest.UserId, requestId);
            }
            catch (Exception notificationEx)
            {
                // Log but don't fail the approval if notification fails
                _logger.LogError(notificationEx,
                    "Failed to send approval notification for request {RequestId}. The approval was still successful.",
                    requestId);
            }

            return await MapToDTO(accessRequest);
        }
        catch (Exception ex)
        {
            // Rollback transaction on any error
            await transaction.RollbackAsync();
            
            _logger.LogError(ex, 
                "Failed to approve access request {RequestId}. Transaction rolled back.", 
                requestId);
            
            throw;
        }
    }

    public async Task<AccessRequestDTO> RejectRequestAsync(
        long requestId, 
        long reviewerUserId, 
        RejectAccessRequestDTO rejectionDto)
    {
        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .FirstOrDefaultAsync(ar => ar.Id == requestId);

        if (accessRequest == null)
        {
            throw new InvalidOperationException("Access request not found");
        }

        // Validate request is in pending status
        if (accessRequest.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot reject request with status: {accessRequest.Status}. Only pending requests can be rejected.");
        }

        // Validate reviewer exists
        var reviewer = await _context.Users.FindAsync(reviewerUserId);
        if (reviewer == null)
        {
            throw new InvalidOperationException("Reviewer user not found");
        }

        // Validate rejection comment is provided
        if (string.IsNullOrWhiteSpace(rejectionDto.ReviewerComment))
        {
            throw new InvalidOperationException("Rejection comment is required");
        }

        // Update access request status
        accessRequest.Status = RequestStatus.Rejected;
        accessRequest.RejectedAt = DateTime.UtcNow;
        accessRequest.RejectedBy = reviewerUserId;
        accessRequest.ProcessedByUserId = reviewerUserId;
        accessRequest.ProcessedAt = DateTime.UtcNow;
        accessRequest.ReviewerComment = rejectionDto.ReviewerComment;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Access request {RequestId} rejected by user {ReviewerId}",
            requestId, reviewerUserId);

        // INTEGRATION: Send notification to the requester
        try
        {
            await _notificationService.SendNotificationAsync(
                accessRequest.UserId,
                "Access Request Rejected ?",
                $"Your request for the '{accessRequest.RequestedRole.Name}' role has been rejected. " +
                $"Reason: {rejectionDto.ReviewerComment}",
                NotificationType.Error,
                "AccessRequest",
                accessRequest.Id);

            _logger.LogInformation(
                "Rejection notification sent to user {UserId} for request {RequestId}",
                accessRequest.UserId, requestId);
        }
        catch (Exception notificationEx)
        {
            // Log but don't fail the rejection if notification fails
            _logger.LogError(notificationEx,
                "Failed to send rejection notification for request {RequestId}. The rejection was still successful.",
                requestId);
        }

        return await MapToDTO(accessRequest);
    }

    public async Task<AccessRequestDTO> CancelRequestAsync(long requestId, long userId)
    {
        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .FirstOrDefaultAsync(ar => ar.Id == requestId);

        if (accessRequest == null)
        {
            throw new InvalidOperationException("Access request not found");
        }

        // Validate the user cancelling is the requester
        if (accessRequest.UserId != userId)
        {
            throw new UnauthorizedAccessException("Only the requester can cancel their own request");
        }

        // Validate request is in pending status
        if (accessRequest.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot cancel request with status: {accessRequest.Status}. Only pending requests can be cancelled.");
        }

        // Update status to cancelled
        accessRequest.Status = RequestStatus.Cancelled;
        accessRequest.ProcessedByUserId = userId;
        accessRequest.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Access request {RequestId} cancelled by user {UserId}",
            requestId, userId);

        return await MapToDTO(accessRequest);
    }

    public async Task<IEnumerable<AccessRequestDTO>> GetPendingRequestsAsync()
    {
        var requests = await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .Where(ar => ar.Status == RequestStatus.Pending)
            .OrderBy(ar => ar.RequestedAt)
            .ToListAsync();

        return await Task.WhenAll(requests.Select(MapToDTO));
    }

    public async Task<IEnumerable<AccessRequestDTO>> GetUserRequestsAsync(long userId)
    {
        var requests = await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .Include(ar => ar.ApprovedByUser)
            .Include(ar => ar.RejectedByUser)
            .Include(ar => ar.ProcessedByUser)
            .Where(ar => ar.UserId == userId)
            .OrderByDescending(ar => ar.RequestedAt)
            .ToListAsync();

        return await Task.WhenAll(requests.Select(MapToDTO));
    }

    public async Task<AccessRequestDTO?> GetRequestByIdAsync(long requestId)
    {
        var request = await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .Include(ar => ar.ApprovedByUser)
            .Include(ar => ar.RejectedByUser)
            .Include(ar => ar.ProcessedByUser)
            .FirstOrDefaultAsync(ar => ar.Id == requestId);

        return request != null ? await MapToDTO(request) : null;
    }

    public async Task<IEnumerable<AccessRequestDTO>> GetAllRequestsAsync(
        string? status = null, 
        long? userId = null, 
        long? roleId = null)
    {
        var query = _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.RequestedRole)
            .Include(ar => ar.ApprovedByUser)
            .Include(ar => ar.RejectedByUser)
            .Include(ar => ar.ProcessedByUser)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RequestStatus>(status, true, out var statusEnum))
        {
            query = query.Where(ar => ar.Status == statusEnum);
        }

        if (userId.HasValue)
        {
            query = query.Where(ar => ar.UserId == userId.Value);
        }

        if (roleId.HasValue)
        {
            query = query.Where(ar => ar.RequestedRoleId == roleId.Value);
        }

        var requests = await query.OrderByDescending(ar => ar.RequestedAt).ToListAsync();

        return await Task.WhenAll(requests.Select(MapToDTO));
    }

    private async Task<AccessRequestDTO> MapToDTO(AccessRequest request)
    {
        // Ensure all navigation properties are loaded
        if (request.User == null)
        {
            await _context.Entry(request).Reference(ar => ar.User).LoadAsync();
        }
        if (request.RequestedRole == null)
        {
            await _context.Entry(request).Reference(ar => ar.RequestedRole).LoadAsync();
        }
        if (request.ApprovedBy.HasValue && request.ApprovedByUser == null)
        {
            await _context.Entry(request).Reference(ar => ar.ApprovedByUser).LoadAsync();
        }
        if (request.RejectedBy.HasValue && request.RejectedByUser == null)
        {
            await _context.Entry(request).Reference(ar => ar.RejectedByUser).LoadAsync();
        }
        if (request.ProcessedByUserId.HasValue && request.ProcessedByUser == null)
        {
            await _context.Entry(request).Reference(ar => ar.ProcessedByUser).LoadAsync();
        }

        return new AccessRequestDTO
        {
            Id = request.Id,
            UserId = request.UserId,
            Username = request.User.Username,
            UserEmail = request.User.Email,
            RequestedRoleId = request.RequestedRoleId,
            RequestedRoleName = request.RequestedRole.Name,
            RequestedPermissions = request.RequestedPermissions,
            Reason = request.Reason,
            Status = request.Status,
            RequestedAt = request.RequestedAt,
            ApprovedAt = request.ApprovedAt,
            ApprovedBy = request.ApprovedBy,
            ApprovedByUsername = request.ApprovedByUser?.Username,
            RejectedAt = request.RejectedAt,
            RejectedBy = request.RejectedBy,
            RejectedByUsername = request.RejectedByUser?.Username,
            ExpiresAt = request.ExpiresAt,
            RequestedDurationHours = request.RequestedDurationHours,
            ReviewerComment = request.ReviewerComment,
            ProcessedByUserId = request.ProcessedByUserId,
            ProcessedByUsername = request.ProcessedByUser?.Username,
            ProcessedAt = request.ProcessedAt
        };
    }
}
