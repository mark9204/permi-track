using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PermiTrack.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class javitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Ensure permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'AccessRequests.Manage')
BEGIN
    INSERT INTO Permissions ([Name], [Resource], [Action], [Description], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES ('AccessRequests.Manage', 'AccessRequests', 'Manage', 'Manage access requests', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

-- Get permission id
DECLARE @permId BIGINT = (SELECT TOP(1) Id FROM Permissions WHERE Name = 'AccessRequests.Manage');

-- Assign permission to Admin/Administrator roles if present and not already assigned
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
SELECT r.Id, @permId, SYSUTCDATETIME()
FROM Roles r
WHERE r.Name IN ('Admin', 'Administrator')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = @permId
  );

-- Fallback: if no Admin role assigned, assign to Role.Id = 1 if present and not already assigned
IF NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    JOIN Roles rr ON rp.RoleId = rr.Id
    WHERE rp.PermissionId = @permId AND rr.Name IN ('Admin', 'Administrator')
)
BEGIN
    IF EXISTS (SELECT 1 FROM Roles WHERE Id = 1)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = 1 AND PermissionId = @permId)
        BEGIN
            INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
            VALUES (1, @permId, SYSUTCDATETIME());
        END
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Remove RolePermission assignments for this permission
DELETE rp
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.Id
WHERE p.Name = 'AccessRequests.Manage';

-- Remove the permission
DELETE FROM Permissions
WHERE Name = 'AccessRequests.Manage';
");
        }
    }
}