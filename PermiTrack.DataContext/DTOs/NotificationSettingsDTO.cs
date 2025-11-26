namespace PermiTrack.DataContext.DTOs;

public class NotificationSettingsDTO
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool NotifyOnRoleChange { get; set; }
    public bool NotifyOnAccessRequest { get; set; }
}
