namespace PermiTrack.DataContext.Enums;

/// <summary>
/// Status of an access request in the approval workflow
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// Request is pending approval
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Request has been approved
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Request has been rejected
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Request was cancelled by the requester or system
    /// </summary>
    Cancelled = 3
}
