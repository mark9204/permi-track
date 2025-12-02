-- Quick verification - Check if HttpAuditLogs table exists
USE [PermiTrackDbContext];
GO

SELECT 
    CASE 
        WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]') AND type = 'U')
        THEN 'HttpAuditLogs EXISTS ?'
        ELSE 'HttpAuditLogs MISSING ?'
    END AS TableStatus;

-- Show all tables in the database
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check migration history
SELECT TOP 10 MigrationId, ProductVersion
FROM [__EFMigrationsHistory]
ORDER BY MigrationId DESC;
