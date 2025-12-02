-- DATABASE VERIFICATION SCRIPT
-- Run this to check if your database schema is correct
-- Execute in SQL Server Management Studio or Azure Data Studio

USE [PermiTrackDbContext];
GO

PRINT '========================================';
PRINT '   DATABASE SCHEMA VERIFICATION REPORT';
PRINT '========================================';
PRINT '';

-- Check if database exists
IF DB_ID('PermiTrackDbContext') IS NOT NULL
BEGIN
    PRINT '? Database [PermiTrackDbContext] exists';
END
ELSE
BEGIN
    PRINT '? Database [PermiTrackDbContext] does NOT exist!';
    RETURN;
END

PRINT '';
PRINT '--- CHECKING ACCESSREQUESTS TABLE ---';

-- Check AccessRequests columns
DECLARE @ProcessedByUserId BIT = 0;
DECLARE @ProcessedAt BIT = 0;
DECLARE @RequestedDurationHours BIT = 0;
DECLARE @ReviewerComment BIT = 0;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedByUserId')
BEGIN
    PRINT '? Column [ProcessedByUserId] exists';
    SET @ProcessedByUserId = 1;
END
ELSE
BEGIN
    PRINT '? Column [ProcessedByUserId] is MISSING!';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedAt')
BEGIN
    PRINT '? Column [ProcessedAt] exists';
    SET @ProcessedAt = 1;
END
ELSE
BEGIN
    PRINT '? Column [ProcessedAt] is MISSING!';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'RequestedDurationHours')
BEGIN
    PRINT '? Column [RequestedDurationHours] exists';
    SET @RequestedDurationHours = 1;
END
ELSE
BEGIN
    PRINT '? Column [RequestedDurationHours] is MISSING!';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ReviewerComment')
BEGIN
    PRINT '? Column [ReviewerComment] exists';
    SET @ReviewerComment = 1;
END
ELSE
BEGIN
    PRINT '? Column [ReviewerComment] is MISSING!';
END

PRINT '';
PRINT '--- CHECKING HTTPAUDITLOGS TABLE ---';

DECLARE @HttpAuditLogsExists BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]') AND type = 'U')
BEGIN
    PRINT '? Table [HttpAuditLogs] exists';
    SET @HttpAuditLogsExists = 1;
    
    -- Count columns
    DECLARE @ColumnCount INT;
    SELECT @ColumnCount = COUNT(*) 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]');
    
    PRINT '  ?? Columns: ' + CAST(@ColumnCount AS VARCHAR(10)) + ' (expected: 12)';
END
ELSE
BEGIN
    PRINT '? Table [HttpAuditLogs] is MISSING!';
END

PRINT '';
PRINT '--- MIGRATION HISTORY ---';

-- Check migration history
IF EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251129140000_AddAccessRequestsAndHttpAuditLogs')
BEGIN
    PRINT '? Migration [20251129140000_AddAccessRequestsAndHttpAuditLogs] is recorded';
END
ELSE
BEGIN
    PRINT '? Migration [20251129140000_AddAccessRequestsAndHttpAuditLogs] is NOT recorded!';
    PRINT '  ?? This migration needs to be applied.';
END

-- Show all applied migrations
PRINT '';
PRINT 'Applied Migrations:';
SELECT TOP 5 
    [MigrationId],
    [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId] DESC;

PRINT '';
PRINT '========================================';
PRINT '             SUMMARY REPORT            ';
PRINT '========================================';

DECLARE @AllGood BIT = 1;

IF @ProcessedByUserId = 0 OR @ProcessedAt = 0 OR @RequestedDurationHours = 0 OR @ReviewerComment = 0
BEGIN
    SET @AllGood = 0;
    PRINT '? AccessRequests table is INCOMPLETE';
    PRINT '  ?? Action: Run MANUAL_MIGRATION_SCRIPT.sql or Update-Database';
END
ELSE
BEGIN
    PRINT '? AccessRequests table is complete';
END

IF @HttpAuditLogsExists = 0
BEGIN
    SET @AllGood = 0;
    PRINT '? HttpAuditLogs table is MISSING';
    PRINT '  ?? Action: Run MANUAL_MIGRATION_SCRIPT.sql or Update-Database';
END
ELSE
BEGIN
    PRINT '? HttpAuditLogs table exists';
END

PRINT '';
IF @AllGood = 1
BEGIN
    PRINT '========================================';
    PRINT '     ??? DATABASE SCHEMA IS CORRECT ???';
    PRINT '========================================';
    PRINT 'Your database is ready! Restart the backend and test.';
END
ELSE
BEGIN
    PRINT '========================================';
    PRINT '     ??? DATABASE SCHEMA HAS ISSUES ???';
    PRINT '========================================';
    PRINT '';
    PRINT 'FIX: Run one of these commands:';
    PRINT '';
    PRINT 'Option 1 (Package Manager Console):';
    PRINT '  Update-Database -Project PermiTrack.DataContext -StartupProject PermiTrack';
    PRINT '';
    PRINT 'Option 2 (SQL Script):';
    PRINT '  Execute the file: MANUAL_MIGRATION_SCRIPT.sql';
END

PRINT '';
PRINT '========================================';
GO
