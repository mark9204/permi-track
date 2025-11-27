using PermiTrack.DataContext.DTOs;

namespace PermiTrack.Services.Interfaces;

/// <summary>
/// Service for managing access request workflow
/// </summary>
public interface IAccessRequestService
{
    /// <summary>
    /// Submit a new access request for a role
    /// </summary>
    /// <param name="userId">ID of the user requesting access</param>
    /// <param name="request">Access request details</param>
    /// <returns>Created access request</returns>
    Task<AccessRequestDTO> SubmitRequestAsync(long userId, SubmitAccessRequestDTO request);

    /// <summary>
    /// Approve an access request and grant the role to the user
    /// </summary>
    /// <param name="requestId">ID of the request to approve</param>
    /// <param name="reviewerUserId">ID of the user approving the request</param>
    /// <param name="approvalDto">Approval details (optional comment and duration override)</param>
    /// <returns>Updated access request</returns>
    Task<AccessRequestDTO> ApproveRequestAsync(long requestId, long reviewerUserId, ApproveAccessRequestDTO? approvalDto = null);

    /// <summary>
    /// Reject an access request
    /// </summary>
    /// <param name="requestId">ID of the request to reject</param>
    /// <param name="reviewerUserId">ID of the user rejecting the request</param>
    /// <param name="rejectionDto">Rejection details (comment required)</param>
    /// <returns>Updated access request</returns>
    Task<AccessRequestDTO> RejectRequestAsync(long requestId, long reviewerUserId, RejectAccessRequestDTO rejectionDto);

    /// <summary>
    /// Cancel a pending access request (by requester)
    /// </summary>
    /// <param name="requestId">ID of the request to cancel</param>
    /// <param name="userId">ID of the user cancelling (must be the requester)</param>
    /// <returns>Updated access request</returns>
    Task<AccessRequestDTO> CancelRequestAsync(long requestId, long userId);

    /// <summary>
    /// Get all pending access requests
    /// </summary>
    /// <returns>List of pending requests</returns>
    Task<IEnumerable<AccessRequestDTO>> GetPendingRequestsAsync();

    /// <summary>
    /// Get all access requests for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's access requests</returns>
    Task<IEnumerable<AccessRequestDTO>> GetUserRequestsAsync(long userId);

    /// <summary>
    /// Get a specific access request by ID
    /// </summary>
    /// <param name="requestId">Request ID</param>
    /// <returns>Access request details</returns>
    Task<AccessRequestDTO?> GetRequestByIdAsync(long requestId);

    /// <summary>
    /// Get all access requests (with optional filtering)
    /// </summary>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="userId">Filter by user (optional)</param>
    /// <param name="roleId">Filter by role (optional)</param>
    /// <returns>List of access requests</returns>
    Task<IEnumerable<AccessRequestDTO>> GetAllRequestsAsync(string? status = null, long? userId = null, long? roleId = null);
}
