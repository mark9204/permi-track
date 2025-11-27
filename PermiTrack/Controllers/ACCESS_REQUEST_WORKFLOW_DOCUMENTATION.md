# Access Request Workflow System Documentation

## Overview

The Access Request Workflow System provides a complete solution for users to request roles and for administrators to approve or reject these requests. The system includes transaction management to ensure data consistency and prevents duplicate role assignments.

## Architecture

### Components

1. **RequestStatus Enum** - Defines the status of access requests
2. **AccessRequest Entity** - Database entity for storing requests
3. **IAccessRequestService** - Service interface
4. **AccessRequestService** - Service implementation with transaction support
5. **AccessRequestWorkflowController** - API endpoints
6. **DTOs** - Data transfer objects for requests and responses

## RequestStatus Enum

```csharp
public enum RequestStatus
{
    Pending = 0,    // Request is awaiting approval
    Approved = 1,   // Request has been approved
    Rejected = 2,   // Request has been rejected
    Cancelled = 3   // Request was cancelled by requester
}
```

## Database Schema

### AccessRequest Table

| Column | Type | Description |
|--------|------|-------------|
| Id | bigint | Primary key |
| UserId | bigint | User requesting access |
| RequestedRoleId | bigint | Role being requested |
| RequestedPermissions | string | JSON of specific permissions (optional) |
| Reason | string | Why the user needs this role |
| Status | enum | Current status (Pending/Approved/Rejected/Cancelled) |
| RequestedAt | DateTime | When request was created |
| ApprovedAt | DateTime? | When request was approved |
| ApprovedBy | bigint? | User who approved |
| RejectedAt | DateTime? | When request was rejected |
| RejectedBy | bigint? | User who rejected |
| ExpiresAt | DateTime? | When granted access expires (null = permanent) |
| RequestedDurationHours | int? | Requested duration in hours |
| ReviewerComment | string? | Comment from approver/rejecter |
| ProcessedByUserId | bigint? | User who processed the request |
| ProcessedAt | DateTime? | When request was processed |
| WorkflowId | bigint? | Approval workflow (optional) |
| CurrentStepId | bigint? | Current workflow step (optional) |

## Key Features

### 1. Transaction Management (Critical!)

The **ApproveRequestAsync** method uses database transactions to ensure atomicity:

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();

try
{
    // 1. Update access request status
    // 2. Create UserRole entry (actual role grant)
    // 3. Save changes
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();  // Both succeed together
}
catch
{
    await transaction.RollbackAsync();  // Both fail together
    throw;
}
```

**Why this matters:**
- Prevents partial updates (request approved but role not granted)
- Ensures data consistency
- Handles race conditions
- All or nothing approach

### 2. Validation

#### Submit Request Validation:
- ? User exists
- ? Role exists
- ? User doesn't already have this role
- ? No pending request for same role exists

#### Approve Request Validation:
- ? Request exists
- ? Request is in Pending status
- ? Reviewer user exists
- ? User still doesn't have the role (race condition check)

#### Reject Request Validation:
- ? Request exists
- ? Request is in Pending status
- ? Reviewer user exists
- ? Rejection comment is provided

### 3. Authorization

- **Submit Request**: Any authenticated user
- **Approve Request**: Requires `AccessRequests.Manage` permission
- **Reject Request**: Requires `AccessRequests.Manage` permission
- **Cancel Request**: Only the requester can cancel their own request
- **View Own Requests**: Any authenticated user
- **View All Requests**: Requires `AccessRequests.Manage` permission

## API Endpoints

### 1. Submit Access Request

```http
POST /api/access-requests/submit
Authorization: Bearer {token}
Content-Type: application/json

{
  "requestedRoleId": 5,
  "reason": "Need access to manage users",
  "requestedDurationHours": 168,
  "requestedPermissions": null
}
```

**Response:**
```json
{
  "message": "Access request submitted successfully",
  "request": {
    "id": 42,
    "userId": 10,
    "username": "john.doe",
    "userEmail": "john@example.com",
    "requestedRoleId": 5,
    "requestedRoleName": "UserManager",
    "reason": "Need access to manage users",
    "status": "Pending",
    "requestedAt": "2024-01-15T10:00:00Z",
    "requestedDurationHours": 168
  }
}
```

### 2. Approve Access Request

```http
PUT /api/access-requests/{id}/approve
Authorization: Bearer {token}
Content-Type: application/json

{
  "reviewerComment": "Approved - temporary access for project",
  "overrideDurationHours": 72
}
```

**Response:**
```json
{
  "message": "Access request approved successfully. Role has been granted to the user.",
  "request": {
    "id": 42,
    "status": "Approved",
    "approvedAt": "2024-01-15T11:00:00Z",
    "approvedBy": 1,
    "approvedByUsername": "admin",
    "processedByUserId": 1,
    "processedByUsername": "admin",
    "processedAt": "2024-01-15T11:00:00Z",
    "reviewerComment": "Approved - temporary access for project",
    "expiresAt": "2024-01-18T11:00:00Z"
  }
}
```

**What happens on approval:**
1. ? Access request status changed to Approved
2. ? UserRole entry created (role actually granted)
3. ? Approval timestamps and reviewer recorded
4. ? Expiration calculated based on duration
5. ? Both operations committed in transaction

### 3. Reject Access Request

```http
PUT /api/access-requests/{id}/reject
Authorization: Bearer {token}
Content-Type: application/json

{
  "reviewerComment": "Insufficient justification for access"
}
```

**Response:**
```json
{
  "message": "Access request rejected",
  "request": {
    "id": 42,
    "status": "Rejected",
    "rejectedAt": "2024-01-15T11:00:00Z",
    "rejectedBy": 1,
    "rejectedByUsername": "admin",
    "processedByUserId": 1,
    "processedByUsername": "admin",
    "processedAt": "2024-01-15T11:00:00Z",
    "reviewerComment": "Insufficient justification for access"
  }
}
```

### 4. Cancel Access Request

```http
PUT /api/access-requests/{id}/cancel
Authorization: Bearer {token}
```

Only the requester can cancel their own pending requests.

**Response:**
```json
{
  "message": "Access request cancelled",
  "request": {
    "id": 42,
    "status": "Cancelled",
    "processedByUserId": 10,
    "processedAt": "2024-01-15T11:00:00Z"
  }
}
```

### 5. Get My Requests

```http
GET /api/access-requests/my-requests
Authorization: Bearer {token}
```

Returns all access requests for the authenticated user.

**Response:**
```json
{
  "userId": 10,
  "totalCount": 3,
  "requests": [
    {
      "id": 42,
      "status": "Pending",
      "requestedRoleName": "UserManager",
      "requestedAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

### 6. Get Pending Requests (Admin)

```http
GET /api/access-requests/pending
Authorization: Bearer {token}
```

Requires `AccessRequests.Manage` permission.

**Response:**
```json
{
  "totalCount": 5,
  "requests": [
    {
      "id": 42,
      "userId": 10,
      "username": "john.doe",
      "requestedRoleId": 5,
      "requestedRoleName": "UserManager",
      "status": "Pending",
      "requestedAt": "2024-01-15T10:00:00Z",
      "reason": "Need access to manage users"
    }
  ]
}
```

### 7. Get Request By ID

```http
GET /api/access-requests/{id}
Authorization: Bearer {token}
```

Users can view their own requests. Managers with `AccessRequests.Manage` can view all.

### 8. Get All Requests (Admin)

```http
GET /api/access-requests?status=Pending&userId=10&roleId=5
Authorization: Bearer {token}
```

Supports filtering by:
- `status` - Filter by status (Pending/Approved/Rejected/Cancelled)
- `userId` - Filter by user
- `roleId` - Filter by role

Requires `AccessRequests.Manage` permission.

### 9. Get Statistics (Admin)

```http
GET /api/access-requests/statistics
Authorization: Bearer {token}
```

Returns aggregate statistics:

```json
{
  "total": 150,
  "pending": 10,
  "approved": 120,
  "rejected": 15,
  "cancelled": 5,
  "averageProcessingTimeHours": 24.5,
  "topRequestedRoles": [
    {
      "roleId": 5,
      "roleName": "UserManager",
      "count": 45
    }
  ]
}
```

## Workflow

### Standard Flow

```
1. User submits request
   ?
2. Status: Pending
   ?
3. Admin reviews request
   ?
4. Admin approves/rejects
   ?
5a. If Approved:
    - Status: Approved
    - Role granted to user (UserRole created)
    - Expiration set (if temporary)
    
5b. If Rejected:
    - Status: Rejected
    - No role granted
    - Rejection comment saved
```

### Cancel Flow

```
1. User submits request
   ?
2. Status: Pending
   ?
3. User changes mind
   ?
4. User cancels request
   ?
5. Status: Cancelled
   (No further action possible)
```

## Service Registration

In `Program.cs`:

```csharp
builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();
```

## Database Migration

Create migration for the updated AccessRequest entity:

```bash
dotnet ef migrations add UpdateAccessRequestWithWorkflow --project PermiTrack.DataContext --startup-project PermiTrack
dotnet ef database update --project PermiTrack.DataContext --startup-project PermiTrack
```

## Required Permissions

Create the following permission in the database:

```sql
INSERT INTO Permissions (Name, Resource, Action, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('AccessRequests.Manage', 'AccessRequests', 'Manage', 'Approve or reject access requests', 1, GETUTCDATE(), GETUTCDATE());

-- Assign to Admin role
DECLARE @PermissionId BIGINT = SCOPE_IDENTITY();
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
VALUES (1, @PermissionId, GETUTCDATE()); -- Assuming role ID 1 is Admin
```

## Error Handling

### Common Errors

1. **"User already has this role"**
   - User is trying to request a role they already have
   - Check current user roles before submitting

2. **"A pending request for this role already exists"**
   - User already has a pending request for this role
   - Wait for current request to be processed or cancel it

3. **"Cannot approve request with status: {status}"**
   - Trying to approve a request that isn't pending
   - Only pending requests can be approved

4. **"User already has this role" (during approval)**
   - Race condition: role was granted after request was submitted
   - Transaction rolls back, no changes made

5. **"Only the requester can cancel their own request"**
   - User trying to cancel someone else's request
   - Only the original requester can cancel

## Transaction Safety

The approval process uses transactions to ensure:

1. **Atomicity**: Either both the request approval AND role grant succeed, or neither does
2. **Consistency**: Database is never in an invalid state
3. **Isolation**: Multiple simultaneous approvals don't conflict
4. **Durability**: Once committed, changes are permanent

### Example Scenario

```
Admin 1 approves request for User A ? Role X
Admin 2 approves request for User A ? Role X (at same time)

Result:
- One approval succeeds (first transaction to commit)
- One approval fails with "User already has this role"
- User A gets Role X exactly once
- No duplicate role assignments
```

## Best Practices

### For Users
1. Provide clear, detailed reasons for access requests
2. Specify accurate duration needs
3. Cancel requests that are no longer needed
4. Check if you already have the role before requesting

### For Administrators
1. Review requests promptly
2. Provide clear comments when rejecting
3. Use duration overrides for temporary access
4. Monitor pending requests regularly

### For Developers
1. Always use transactions for critical operations
2. Validate status before state changes
3. Log all approval/rejection actions
4. Handle race conditions gracefully
5. Test concurrent approval scenarios

## Monitoring

Key metrics to monitor:

- **Pending request count**: Should be low
- **Average processing time**: Should be reasonable
- **Approval rate**: Track approval vs rejection ratio
- **Most requested roles**: Identify common needs
- **Request by user**: Detect unusual patterns

## Security Considerations

1. **Permission Checks**: All management endpoints require `AccessRequests.Manage`
2. **User Isolation**: Users can only see their own requests (unless they're admins)
3. **Cancel Authorization**: Only requester can cancel their requests
4. **Audit Trail**: All actions are logged with user ID and timestamp
5. **Transaction Integrity**: Prevents inconsistent state

## Future Enhancements

Potential improvements:

1. **Multi-Step Approvals**: Support workflow with multiple approval stages
2. **Approval Delegation**: Allow approvers to delegate
3. **Auto-Expiration**: Automatically revoke expired access
4. **Notification System**: Email notifications for status changes
5. **Approval Comments**: Support multiple comments/discussion
6. **Bulk Operations**: Approve/reject multiple requests at once
7. **Request Templates**: Pre-defined request templates for common scenarios
8. **Approval Rules**: Automatic approval based on rules
