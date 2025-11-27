namespace PermiTrack.DataContext.Enums;

/// <summary>
/// Type of notification to determine severity and icon
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Informational notification
    /// </summary>
    Info = 0,

    /// <summary>
    /// Success notification (e.g., request approved)
    /// </summary>
    Success = 1,

    /// <summary>
    /// Warning notification (e.g., expiring access)
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error notification (e.g., request rejected)
    /// </summary>
    Error = 3
}
