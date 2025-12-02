-- Add a new role to the Roles table
USE [PermiTrackDbContext];
GO

-- Ensure you are not inserting a duplicate role name
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = 'SuperAdmin')
BEGIN
    INSERT INTO [dbo].[Roles] 
        ([Name], [Description], [IsActive], [CreatedAt], [UpdatedAt], [Level], [ParentRoleId])
    VALUES 
        ('SuperAdmin', 'This is a super administrator role with the highest level of permissions.', 1, GETUTCDATE(), GETUTCDATE(), 100, NULL);
    
    PRINT 'Role "SuperAdmin" created successfully.';
END
ELSE
BEGIN
    PRINT 'Role "SuperAdmin" already exists.';
END
GO

-- Verify that the role has been added
SELECT TOP 10 * 
FROM [dbo].[Roles] 
ORDER BY CreatedAt DESC;
GO

