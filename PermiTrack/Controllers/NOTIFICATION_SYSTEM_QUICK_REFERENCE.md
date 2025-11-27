# Notification System - Quick Reference

## Quick Start

### 1. Get My Notifications

```http
GET /api/notifications/my-notifications
Authorization: Bearer {token}
```

### 2. Get Unread Count (for Bell Icon)

```http
GET /api/notifications/unread-count
Authorization: Bearer {token}

Response: { "unreadCount": 5 }
```

### 3. Mark as Read

```http
PUT /api/notifications/{id}/mark-as-read
Authorization: Bearer {token}
```

### 4. Mark All as Read

```http
PUT /api/notifications/mark-all-as-read
Authorization: Bearer {token}
```

### 5. Delete Notification

```http
DELETE /api/notifications/{id}
Authorization: Bearer {token}
```

## Notification Types

```csharp
Info = 0,      // ?? Blue - System info
Success = 1,   // ? Green - Success messages
Warning = 2,   // ?? Yellow - Warnings
Error = 3      // ? Red - Errors/Rejections
```

## Access Request Integration

### Automatic Notifications

**When Request Approved:**
```
Title: "Access Request Approved! ?"
Message: "Your request for the 'RoleName' role has been approved!"
Type: Success
```

**When Request Rejected:**
```
Title: "Access Request Rejected ?"
Message: "Your request for the 'RoleName' role has been rejected. 
          Reason: {comment}"
Type: Error
```

## Common Endpoints

### Get Only Unread Notifications
```http
GET /api/notifications/my-notifications?unreadOnly=true
```

### Get More Notifications
```http
GET /api/notifications/my-notifications?pageSize=100
```

### Clear All Read Notifications
```http
DELETE /api/notifications/clear-read
```

## Response Format

```json
{
  "id": 123,
  "userId": 10,
  "title": "Access Request Approved! ?",
  "message": "Your request has been approved!",
  "type": "Success",
  "isRead": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "readAt": null,
  "relatedResourceType": "AccessRequest",
  "relatedResourceId": 42
}
```

## Integration Example (AccessRequestService)

### Constructor
```csharp
public AccessRequestService(
    PermiTrackDbContext context,
    ILogger<AccessRequestService> logger,
    INotificationService notificationService)  // ? Inject
{
    _context = context;
    _logger = logger;
    _notificationService = notificationService;  // ? Store
}
```

### On Approval (After Transaction Commit)
```csharp
// After successful transaction commit
await transaction.CommitAsync();

// Send notification
try
{
    await _notificationService.SendNotificationAsync(
        accessRequest.UserId,
        "Access Request Approved! ?",
        $"Your request for the '{roleName}' role has been approved!",
        NotificationType.Success,
        "AccessRequest",
        accessRequest.Id);
}
catch (Exception ex)
{
    // Log but don't fail the approval
    _logger.LogError(ex, "Failed to send notification");
}
```

### On Rejection
```csharp
// After updating request status
await _context.SaveChangesAsync();

// Send notification
try
{
    await _notificationService.SendNotificationAsync(
        accessRequest.UserId,
        "Access Request Rejected ?",
        $"Reason: {rejectionComment}",
        NotificationType.Error,
        "AccessRequest",
        accessRequest.Id);
}
catch (Exception ex)
{
    // Log but don't fail the rejection
    _logger.LogError(ex, "Failed to send notification");
}
```

## Service Methods

### Send Notification
```csharp
await _notificationService.SendNotificationAsync(
    userId: 10,
    title: "Title",
    message: "Message",
    type: NotificationType.Info,
    relatedResourceType: "AccessRequest",  // Optional
    relatedResourceId: 42,                 // Optional
    expiresAt: DateTime.UtcNow.AddDays(7) // Optional
);
```

### Get Notifications
```csharp
var notifications = await _notificationService.GetMyNotificationsAsync(
    userId: 10,
    unreadOnly: true,
    pageSize: 50
);
```

### Mark as Read
```csharp
var notification = await _notificationService.MarkAsReadAsync(
    notificationId: 123,
    userId: 10
);
```

### Get Unread Count
```csharp
var count = await _notificationService.GetUnreadCountAsync(userId: 10);
```

## File Structure

```
PermiTrack/
??? Controllers/
?   ??? NotificationsController.cs  (NEW)
?   ??? NOTIFICATION_SYSTEM_DOCUMENTATION.md
??? ...

PermiTrack.DataContext/
??? Entites/
?   ??? Notifications.cs  (Updated with enum)
??? Enums/
?   ??? NotificationType.cs  (NEW)
??? DTOs/
    ??? CreateNotificationDTO.cs  (NEW)
    ??? NotificationDTO.cs  (NEW)

PermiTrack.Services/
??? Interfaces/
?   ??? INotificationService.cs  (NEW)
??? Services/
    ??? NotificationService.cs  (NEW)
    ??? AccessRequestService.cs  (Updated with integration)
```

## Service Registration

Already configured in `Program.cs`:

```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Frontend Integration

### Display Notification Badge
```javascript
async function getUnreadCount() {
  const response = await fetch('/api/notifications/unread-count', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const data = await response.json();
  document.querySelector('.badge').textContent = data.unreadCount;
}
```

### Load Notifications
```javascript
async function loadNotifications(unreadOnly = false) {
  const response = await fetch(
    `/api/notifications/my-notifications?unreadOnly=${unreadOnly}`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  const data = await response.json();
  
  data.notifications.forEach(n => {
    addNotificationToUI(n);
  });
}
```

### Mark as Read on Click
```javascript
async function handleNotificationClick(notificationId) {
  await fetch(`/api/notifications/${notificationId}/mark-as-read`, {
    method: 'PUT',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  // Update UI
  updateNotificationBadge();
}
```

## Database Schema

```sql
CREATE TABLE Notifications (
    Id BIGINT PRIMARY KEY IDENTITY,
    UserId BIGINT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    ReadAt DATETIME2 NULL,
    ExpiresAt DATETIME2 NULL,
    RelatedResourceType NVARCHAR(100) NULL,
    RelatedResourceId BIGINT NULL,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    INDEX IX_Notifications_UserId (UserId),
    INDEX IX_Notifications_UserId_IsRead (UserId, IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt)
);
```

## Key Features

? **Auto-Integration** - Automatic notifications on approval/rejection  
? **Unread Count** - Perfect for bell icon badge  
? **Mark as Read** - Individual or bulk operations  
? **Resource Linking** - Link notifications to related entities  
? **Type-Based** - Different types for different severity  
? **Pagination** - Efficient loading of notifications  
? **User Isolation** - Users only see their own notifications  
? **Error Resilient** - Notification failures don't break workflows  

## Testing Checklist

### Basic Operations
- [ ] Get my notifications
- [ ] Get unread count
- [ ] Mark notification as read
- [ ] Mark all as read
- [ ] Delete notification
- [ ] Clear all read notifications

### Access Request Integration
- [ ] Submit access request
- [ ] Approve request
- [ ] Verify requester receives success notification
- [ ] Reject request
- [ ] Verify requester receives error notification
- [ ] Check notification links to access request

### Authorization
- [ ] User can only see own notifications
- [ ] User can only mark own notifications as read
- [ ] User can only delete own notifications
- [ ] Admin can send notifications with permission

### UI Integration
- [ ] Bell icon shows unread count
- [ ] Clicking notification marks it as read
- [ ] Notifications update in real-time (or on refresh)
- [ ] Notification types display with correct icons

## Common Patterns

### Check if Notification Failed (Non-Breaking)
```csharp
try
{
    await _notificationService.SendNotificationAsync(...);
}
catch (Exception ex)
{
    // Log but don't throw - allow operation to continue
    _logger.LogError(ex, "Failed to send notification");
}
```

### Send Notification with Expiration
```csharp
await _notificationService.SendNotificationAsync(
    userId,
    "Limited Time Offer",
    "This offer expires in 24 hours",
    NotificationType.Warning,
    expiresAt: DateTime.UtcNow.AddHours(24)
);
```

### Send Notification with Resource Link
```csharp
await _notificationService.SendNotificationAsync(
    userId,
    "Access Request Updated",
    "Your request status has changed",
    NotificationType.Info,
    relatedResourceType: "AccessRequest",
    relatedResourceId: requestId
);
```

## Error Handling

### User Not Found
```json
{
  "message": "User with ID 999 not found"
}
```

### Notification Not Found
```json
{
  "message": "Notification not found or does not belong to this user"
}
```

### Permission Denied (Admin Send)
```json
{
  "message": "You do not have permission to send notifications"
}
```

## Performance Tips

1. **Pagination**: Use `pageSize` parameter (max 100)
2. **Unread Only**: Filter with `unreadOnly=true`
3. **Indexes**: Database has indexes for fast queries
4. **Cleanup**: Periodically delete old read notifications

## Database Migration

```bash
# Create migration
dotnet ef migrations add UpdateNotificationSystem --project ..\PermiTrack.DataContext --startup-project .

# Apply migration
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

## Create Required Permission

```sql
INSERT INTO Permissions (Name, Resource, Action, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('Notifications.Send', 'Notifications', 'Send', 
        'Send notifications to users', 1, GETUTCDATE(), GETUTCDATE());

DECLARE @PermissionId BIGINT = SCOPE_IDENTITY();
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
VALUES (1, @PermissionId, GETUTCDATE());
```

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Notification created |
| 400 | Bad request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Notification not found |
| 500 | Server error |

## Next Steps

1. ? Service and DTOs created
2. ? Controller implemented
3. ? Service registered
4. ? Integrated with AccessRequestService
5. ?? Create database migration
6. ?? Create Notifications.Send permission
7. ?? Test all endpoints
8. ?? Integrate with frontend
9. ?? Add real-time updates (SignalR)
10. ?? Add email notifications
