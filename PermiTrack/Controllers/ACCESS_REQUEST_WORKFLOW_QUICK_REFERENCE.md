# Access Request Workflow - Quick Reference

## Quick Start

### 1. Submit a Request (User)

```http
POST /api/access-requests/submit
Authorization: Bearer {your-token}

{
  "requestedRoleId": 5,
  "reason": "Need access to manage users for Q1 project",
  "requestedDurationHours": 720
}
```

### 2. View My Requests (User)

```http
GET /api/access-requests/my-requests
Authorization: Bearer {your-token}
```

### 3. Cancel My Request (User)

```http
PUT /api/access-requests/{id}/cancel
Authorization: Bearer {your-token}
```

### 4. View Pending Requests (Admin)

```http
GET /api/access-requests/pending
Authorization: Bearer {admin-token}
```

### 5. Approve Request (Admin)

```http
PUT /api/access-requests/{id}/approve
Authorization: Bearer {admin-token}

{
  "reviewerComment": "Approved for project duration",
  "overrideDurationHours": 168
}
```

### 6. Reject Request (Admin)

```http
PUT /api/access-requests/{id}/reject
Authorization: Bearer {admin-token}

{
  "reviewerComment": "Please provide more details about why you need this access"
}
```

## Request Status Flow

```
???????????
? PENDING ? ???? Initial status when submitted
???????????
     ?
     ???? APPROVED  (Role granted to user)
     ?
     ???? REJECTED  (No role granted)
     ?
     ???? CANCELLED (User cancelled)
```

## DTOs

### SubmitAccessRequestDTO
```csharp
{
  "requestedRoleId": long,           // Required
  "reason": string,                  // Required
  "requestedDurationHours": int?,    // Optional (null = permanent)
  "requestedPermissions": string?    // Optional (JSON)
}
```

### ApproveAccessRequestDTO
```csharp
{
  "reviewerComment": string?,        // Optional
  "overrideDurationHours": int?      // Optional (overrides requested)
}
```

### RejectAccessRequestDTO
```csharp
{
  "reviewerComment": string          // Required
}
```

## Common Scenarios

### User Needs Temporary Access
```json
{
  "requestedRoleId": 5,
  "reason": "Working on migration project",
  "requestedDurationHours": 168  // 1 week
}
```

### User Needs Permanent Access
```json
{
  "requestedRoleId": 5,
  "reason": "Promoted to team lead",
  "requestedDurationHours": null  // null = permanent
}
```

### Admin Approves with Different Duration
```json
{
  "reviewerComment": "Approved for 3 days instead of requested week",
  "overrideDurationHours": 72  // 3 days
}
```

### Admin Rejects Request
```json
{
  "reviewerComment": "This role is not appropriate for your current position. Please discuss with your manager first."
}
```

## Permissions Required

| Action | Permission | Who Can Do It |
|--------|-----------|---------------|
| Submit Request | (Any authenticated user) | Users |
| View Own Requests | (Any authenticated user) | Users |
| Cancel Own Request | (Any authenticated user) | Requester only |
| Approve Request | `AccessRequests.Manage` | Admins |
| Reject Request | `AccessRequests.Manage` | Admins |
| View All Requests | `AccessRequests.Manage` | Admins |
| View Pending Requests | `AccessRequests.Manage` | Admins |
| View Statistics | `AccessRequests.Manage` | Admins |

## File Structure

```
PermiTrack/
??? Controllers/
?   ??? AccessRequestWorkflowController.cs  (NEW)
?   ??? AccessRequestsController.cs  (Existing workflow controller)
?   ??? ACCESS_REQUEST_WORKFLOW_DOCUMENTATION.md
??? ...

PermiTrack.DataContext/
??? Entites/
?   ??? AccessRequests.cs  (Updated with new fields)
??? Enums/
?   ??? RequestStatus.cs  (NEW)
??? DTOs/
    ??? SubmitAccessRequestDTO.cs  (NEW)
    ??? ApproveAccessRequestDTO.cs  (NEW)
    ??? RejectAccessRequestDTO.cs  (NEW)
    ??? AccessRequestDTO.cs  (Updated)

PermiTrack.Services/
??? Interfaces/
?   ??? IAccessRequestService.cs  (NEW)
??? Services/
    ??? AccessRequestService.cs  (NEW)
```

## Service Registration

Already configured in `Program.cs`:

```csharp
builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();
```

## Transaction Handling

**CRITICAL**: The approve method uses database transactions:

```csharp
// Start transaction
using var transaction = await _context.Database.BeginTransactionAsync();

try {
    // 1. Update request status
    // 2. Create UserRole (grant access)
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();  // ? Both succeed
}
catch {
    await transaction.RollbackAsync();  // ? Both fail
    throw;
}
```

**Why this matters:**
- Prevents partial updates
- Ensures data consistency
- Request approval + role grant happen together or not at all

## Validation Rules

### Submit Request
? User exists  
? Role exists  
? User doesn't already have this role  
? No pending request for same role  

### Approve Request
? Request exists  
? Request status is Pending  
? Reviewer user exists  
? User still doesn't have the role (race condition check)  

### Reject Request
? Request exists  
? Request status is Pending  
? Reviewer user exists  
? Rejection comment provided  

### Cancel Request
? Request exists  
? Request status is Pending  
? User is the requester (not someone else)  

## Error Messages

| Error | Meaning | Solution |
|-------|---------|----------|
| "User already has this role" | User has the role already | Check UserRoles table |
| "A pending request for this role already exists" | Duplicate pending request | Cancel existing or wait |
| "Cannot approve request with status: {status}" | Request isn't pending | Only pending requests can be approved |
| "Rejection comment is required" | Missing rejection reason | Provide reviewerComment |
| "Only the requester can cancel their own request" | Unauthorized cancel | Only original requester can cancel |

## Testing Checklist

### User Flow
- [ ] Submit request successfully
- [ ] View own requests
- [ ] Cancel pending request
- [ ] Cannot cancel approved/rejected request
- [ ] Cannot submit duplicate request
- [ ] Cannot request role already have

### Admin Flow
- [ ] View all pending requests
- [ ] Approve request successfully
- [ ] Role is granted to user (check UserRoles)
- [ ] Cannot approve non-pending request
- [ ] Reject request with comment
- [ ] Cannot reject non-pending request
- [ ] View all requests with filters
- [ ] View statistics

### Transaction Safety
- [ ] Approval creates UserRole entry
- [ ] Both request update and role grant succeed together
- [ ] If error occurs, transaction rolls back
- [ ] No partial updates in database

### Duration Handling
- [ ] Permanent access (null duration)
- [ ] Temporary access (duration specified)
- [ ] Duration override on approval
- [ ] ExpiresAt calculated correctly

## Quick SQL Queries

### Check User's Access Requests
```sql
SELECT * FROM AccessRequests 
WHERE UserId = {userId}
ORDER BY RequestedAt DESC;
```

### Check Pending Requests
```sql
SELECT * FROM AccessRequests 
WHERE Status = 'Pending'
ORDER BY RequestedAt;
```

### Check If Role Was Granted
```sql
SELECT * FROM UserRoles
WHERE UserId = {userId} AND RoleId = {roleId} AND IsActive = 1;
```

### View Request History for Role
```sql
SELECT ar.*, u.Username, r.Name as RoleName
FROM AccessRequests ar
JOIN Users u ON ar.UserId = u.Id
JOIN Roles r ON ar.RequestedRoleId = r.Id
WHERE ar.RequestedRoleId = {roleId}
ORDER BY ar.RequestedAt DESC;
```

## Database Migration

```bash
# Create migration
dotnet ef migrations add UpdateAccessRequestWorkflow --project ..\PermiTrack.DataContext --startup-project .

# Apply migration
dotnet ef database update --project ..\PermiTrack.DataContext --startup-project .
```

## Create Required Permission

```sql
-- Create permission
INSERT INTO Permissions (Name, Resource, Action, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('AccessRequests.Manage', 'AccessRequests', 'Manage', 
        'Approve or reject access requests', 1, GETUTCDATE(), GETUTCDATE());

-- Get permission ID
DECLARE @PermissionId BIGINT = SCOPE_IDENTITY();

-- Assign to Admin role (assuming role ID 1)
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
VALUES (1, @PermissionId, GETUTCDATE());
```

## Example Usage

### User Submits Request
```bash
curl -X POST https://api.example.com/api/access-requests/submit \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "requestedRoleId": 5,
    "reason": "Need access for Q1 project",
    "requestedDurationHours": 720
  }'
```

### Admin Approves
```bash
curl -X PUT https://api.example.com/api/access-requests/42/approve \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "reviewerComment": "Approved",
    "overrideDurationHours": 168
  }'
```

### Admin Views Pending
```bash
curl -X GET https://api.example.com/api/access-requests/pending \
  -H "Authorization: Bearer {admin-token}"
```

## Status Codes

| Code | Meaning |
|------|---------|
| 201 | Request created successfully |
| 200 | Operation successful |
| 400 | Bad request (validation failed) |
| 401 | Unauthorized (no valid token) |
| 403 | Forbidden (no permission) |
| 404 | Request not found |
| 500 | Server error |

## Next Steps

1. ? Service and DTOs created
2. ? Controller implemented
3. ? Service registered
4. ?? Create database migration
5. ?? Create AccessRequests.Manage permission
6. ?? Test all endpoints
7. ?? Test transaction safety
8. ?? Document API in Swagger
