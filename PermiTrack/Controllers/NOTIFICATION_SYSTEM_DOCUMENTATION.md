# Notification System Documentation

## Overview

The Notification System provides real-time alerts to users about important events in the application, such as access request approvals, rejections, role assignments, and system events. The system is fully integrated with the Access Request Workflow.

## Architecture

### Components

1. **NotificationType Enum** - Defines notification severity levels
2. **Notification Entity** - Database entity for storing notifications
3. **INotificationService** - Service interface
4. **NotificationService** - Service implementation
5. **NotificationsController** - API endpoints
6. **Access Request Integration** - Automatic notifications on approval/rejection

## NotificationType Enum

```csharp
public enum NotificationType
{
    Info = 0,      // Informational (blue icon)
    Success = 1,   // Success (green icon, checkmark)
    Warning = 2,   // Warning (yellow icon, exclamation)
    Error = 3      // Error/Rejection (red icon, X)
}
```

## Database Schema

### Notification Table

| Column | Type | Description |
|--------|------|-------------|
| Id | bigint | Primary key |
| UserId | bigint | User who receives the notification |
| Title | string | Notification title (max 200 chars) |
| Message | string | Notification message content |
| Type | enum | Notification type (Info/Success/Warning/Error) |
| IsRead | bool | Whether notification has been read |
| CreatedAt | DateTime | When notification was created |
| ReadAt | DateTime? | When notification was read |
| ExpiresAt | DateTime? | When notification expires (optional) |
| RelatedResourceType | string? | Type of related resource (e.g., "AccessRequest") |
| RelatedResourceId | bigint? | ID of related resource |

### Indexes

- Primary key on `Id`
- Index on `UserId` (for user queries)
- Composite index on `UserId, IsRead` (for unread count)
- Index on `CreatedAt` (for time-based queries)

## Key Features

### 1. Real-Time Notifications

Users receive immediate notifications about:
- ? Access request approvals
- ? Access request rejections
- ?? Role assignments
- ?? Expiring access
- ?? System announcements

### 2. Unread Count (Bell Icon)

Perfect for displaying notification badges:
```http
GET /api/notifications/unread-count
Response: { "userId": 10, "unreadCount": 5 }
```

### 3. Mark as Read

Individual or bulk operations:
- Mark single notification as read
- Mark all notifications as read

### 4. Resource Linking

Notifications can link to related resources:
- `RelatedResourceType`: "AccessRequest", "UserRole", etc.
- `RelatedResourceId`: ID of the related entity

### 5. Auto-Integration with Access Requests

**Automatic notifications sent when:**

#### On Approval:
```
Title: "Access Request Approved! ?"
Message: "Your request for the 'UserManager' role has been approved! 
          You now have access to this role."
Type: Success
```

#### On Rejection:
```
Title: "Access Request Rejected ?"
Message: "Your request for the 'UserManager' role has been rejected. 
          Reason: Insufficient justification."
Type: Error
```

## API Endpoints

### 1. Get My Notifications

```http
GET /api/notifications/my-notifications?unreadOnly=false&pageSize=50
Authorization: Bearer {token}
```

**Query Parameters:**
- `unreadOnly` (bool, default: false) - Only return unread notifications
- `pageSize` (int, default: 50, max: 100) - Number of notifications to return

**Response:**
```json
{
  "userId": 10,
  "unreadOnly": false,
  "count": 25,
  "notifications": [
    {
      "id": 123,
      "userId": 10,
      "title": "Access Request Approved! ?",
      "message": "Your request for the 'UserManager' role has been approved!",
      "type": "Success",
      "isRead": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "readAt": null,
      "expiresAt": null,
      "relatedResourceType": "AccessRequest",
      "relatedResourceId": 42
    }
  ]
}
```

### 2. Get Unread Count

```http
GET /api/notifications/unread-count
Authorization: Bearer {token}
```

**Response:**
```json
{
  "userId": 10,
  "unreadCount": 5
}
```

**Usage:** Display badge on bell icon showing number of unread notifications.

### 3. Get Notification By ID

```http
GET /api/notifications/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 123,
  "userId": 10,
  "title": "Access Request Approved! ?",
  "message": "Your request for the 'UserManager' role has been approved!",
  "type": "Success",
  "isRead": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "readAt": null,
  "expiresAt": null,
  "relatedResourceType": "AccessRequest",
  "relatedResourceId": 42
}
```

### 4. Mark Notification as Read

```http
PUT /api/notifications/{id}/mark-as-read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Notification marked as read",
  "notification": {
    "id": 123,
    "isRead": true,
    "readAt": "2024-01-15T11:00:00Z"
  }
}
```

### 5. Mark All as Read

```http
PUT /api/notifications/mark-all-as-read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Marked 5 notifications as read",
  "markedCount": 5
}
```

### 6. Delete Notification

```http
DELETE /api/notifications/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Notification deleted successfully"
}
```

### 7. Clear Read Notifications

```http
DELETE /api/notifications/clear-read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Deleted 10 read notifications",
  "deletedCount": 10
}
```

### 8. Send Notification (Admin)

```http
POST /api/notifications/send?userId=10
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "title": "System Maintenance",
  "message": "The system will be down for maintenance on Sunday at 2 AM",
  "type": "Warning",
  "expiresAt": "2024-01-20T00:00:00Z"
}
```

**Permission Required:** `Notifications.Send`

**Response:**
```json
{
  "message": "Notification sent successfully",
  "notification": {
    "id": 456,
    "userId": 10,
    "title": "System Maintenance",
    "type": "Warning"
  }
}
```

## Integration with Access Request Workflow

### ApproveRequestAsync Integration

```csharp
public async Task<AccessRequestDTO> ApproveRequestAsync(
    long requestId, 
    long reviewerUserId, 
    ApproveAccessRequestDTO? approvalDto = null)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // 1. Update access request
        // 2. Grant role to user
        // 3. Save changes
        await _context.SaveChangesAsync();
        
        // 4. Commit transaction
        await transaction.CommitAsync();

        // 5. SEND NOTIFICATION (after successful commit)
        try
        {
            await _notificationService.SendNotificationAsync(
                accessRequest.UserId,
                "Access Request Approved! ?",
                $"Your request for the '{accessRequest.RequestedRole.Name}' role has been approved!",
                NotificationType.Success,
                "AccessRequest",
                accessRequest.Id);
        }
        catch (Exception notificationEx)
        {
            // Log but don't fail approval if notification fails
            _logger.LogError(notificationEx, "Failed to send notification");
        }

        return await MapToDTO(accessRequest);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

**Key Points:**
- ? Notification sent AFTER transaction commits
- ? If notification fails, approval still succeeds
- ? Error logged but not thrown
- ? User receives notification even if they're offline

### RejectRequestAsync Integration

```csharp
public async Task<AccessRequestDTO> RejectRequestAsync(
    long requestId, 
    long reviewerUserId, 
    RejectAccessRequestDTO rejectionDto)
{
    // 1. Update access request status
    accessRequest.Status = RequestStatus.Rejected;
    accessRequest.ReviewerComment = rejectionDto.ReviewerComment;
    
    // 2. Save changes
    await _context.SaveChangesAsync();

    // 3. SEND NOTIFICATION
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
    }
    catch (Exception notificationEx)
    {
        // Log but don't fail rejection if notification fails
        _logger.LogError(notificationEx, "Failed to send notification");
    }

    return await MapToDTO(accessRequest);
}
```

## Service Registration

In `Program.cs`:

```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Usage Examples

### Frontend - Display Notification Badge

```javascript
// Poll for unread count every 30 seconds
setInterval(async () => {
  const response = await fetch('/api/notifications/unread-count', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const data = await response.json();
  updateBellIcon(data.unreadCount); // Update UI
}, 30000);
```

### Frontend - Display Notifications

```javascript
async function loadNotifications() {
  const response = await fetch(
    '/api/notifications/my-notifications?unreadOnly=true&pageSize=10',
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  const data = await response.json();
  
  data.notifications.forEach(notification => {
    displayNotification(notification);
  });
}
```

### Frontend - Mark as Read When Clicked

```javascript
async function markAsRead(notificationId) {
  await fetch(`/api/notifications/${notificationId}/mark-as-read`, {
    method: 'PUT',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  // Reload notifications or update UI
  loadNotifications();
}
```

### Admin - Send System Announcement

```javascript
async function sendAnnouncement(userId, message) {
  await fetch(`/api/notifications/send?userId=${userId}`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'System Announcement',
      message: message,
      type: 'Info'
    })
  });
}
```

## Notification Types and Use Cases

### Info (Blue, ??)
- System announcements
- Feature updates
- Reminders
- General information

### Success (Green, ?)
- Access request approved
- Role granted
- Action completed successfully
- Verification successful

### Warning (Yellow, ??)
- Access expiring soon
- Pending action required
- System maintenance scheduled
- Configuration changes needed

### Error (Red, ?)
- Access request rejected
- Action failed
- Permission denied
- System error

## Database Migration

Create migration for the updated Notification entity:

```bash
dotnet ef migrations add UpdateNotificationWithEnum --project ..\PermiTrack.DataContext --startup-project .
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

## Required Permissions

Create the permission for sending notifications:

```sql
INSERT INTO Permissions (Name, Resource, Action, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('Notifications.Send', 'Notifications', 'Send', 
        'Send notifications to users', 1, GETUTCDATE(), GETUTCDATE());

-- Assign to Admin role
DECLARE @PermissionId BIGINT = SCOPE_IDENTITY();
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
VALUES (1, @PermissionId, GETUTCDATE());
```

## Best Practices

### For Developers

1. **Error Handling**: Always wrap notification sending in try-catch
2. **Transaction Safety**: Send notifications AFTER transaction commits
3. **Non-Blocking**: Don't fail operations if notification fails
4. **Logging**: Always log notification failures
5. **Resource Linking**: Include related resource info when possible

### For Users

1. **Check Regularly**: Review notifications frequently
2. **Mark as Read**: Keep inbox clean by marking read notifications
3. **Clear Read**: Periodically clear old read notifications
4. **Link Following**: Click notifications to view related resources

### For Administrators

1. **Monitor Unread**: Track users with many unread notifications
2. **System Announcements**: Use for important communications
3. **Expiration**: Set expiration for time-sensitive notifications
4. **Type Selection**: Choose appropriate notification type

## Performance Considerations

### Database Optimization

- Indexes on `UserId, IsRead` for fast unread count
- Index on `CreatedAt` for time-based queries
- Pagination (max 100 per request)

### Auto-Cleanup

Consider implementing auto-cleanup for:
- Expired notifications (ExpiresAt < now)
- Old read notifications (> 30 days)
- Excessive notifications (> 1000 per user)

### Example Cleanup Job

```csharp
// Delete expired notifications
var expired = await _context.Notifications
    .Where(n => n.ExpiresAt != null && n.ExpiresAt < DateTime.UtcNow)
    .ToListAsync();

_context.Notifications.RemoveRange(expired);
await _context.SaveChangesAsync();
```

## Security Considerations

1. **Authorization**: Users can only see their own notifications
2. **Validation**: Validate user exists before sending notification
3. **Rate Limiting**: Consider rate limiting notification sends
4. **Content**: Sanitize notification content (prevent XSS)
5. **Admin Permission**: Only admins can send arbitrary notifications

## Future Enhancements

Potential improvements:

1. **Email Integration**: Send email for important notifications
2. **Push Notifications**: WebSocket/SignalR for real-time updates
3. **Notification Preferences**: User settings for notification types
4. **Grouping**: Group related notifications
5. **Templates**: Predefined notification templates
6. **Rich Content**: Support for markdown or HTML in messages
7. **Attachments**: Link files or documents to notifications
8. **Read Receipts**: Track when notifications are actually viewed

## Troubleshooting

### Issue: Notifications not appearing

**Check:**
1. User ID is correct
2. Notification was saved to database
3. User has permission to view notifications
4. Query is filtering correctly

### Issue: Unread count not updating

**Check:**
1. Notifications are being marked as read
2. ReadAt timestamp is set
3. Indexes are present on the table
4. Cache is not stale (if using caching)

### Issue: Notification sending fails

**Check:**
1. User exists in database
2. Service is registered in DI container
3. Database connection is available
4. Check error logs for details

## Monitoring

Key metrics to track:

- **Notifications Created**: Per hour/day
- **Average Unread Count**: Per user
- **Read Rate**: % of notifications that get read
- **Response Time**: Time to mark as read after creation
- **Failed Sends**: Count of failed notification attempts
