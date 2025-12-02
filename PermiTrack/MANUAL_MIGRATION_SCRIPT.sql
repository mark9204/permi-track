-- MANUAL MIGRATION SCRIPT
-- Run this in SQL Server Management Studio or Azure Data Studio
-- Connected to your PermiTrackDbContext database

-- Use the correct database
USE [PermiTrackDbContext];
GO

-- =========================================================
-- STEP 1: Add missing columns to AccessRequests table
-- =========================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedAt')
BEGIN
    ALTER TABLE [AccessRequests]
    ADD [ProcessedAt] datetime2(7) NULL;
    PRINT 'Added ProcessedAt column to AccessRequests';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedByUserId')
BEGIN
    ALTER TABLE [AccessRequests]
    ADD [ProcessedByUserId] bigint NULL;
    PRINT 'Added ProcessedByUserId column to AccessRequests';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'RequestedDurationHours')
BEGIN
    ALTER TABLE [AccessRequests]
    ADD [RequestedDurationHours] int NULL;
    PRINT 'Added RequestedDurationHours column to AccessRequests';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ReviewerComment')
BEGIN
    ALTER TABLE [AccessRequests]
    ADD [ReviewerComment] nvarchar(1000) NULL;
    PRINT 'Added ReviewerComment column to AccessRequests';
END
GO

-- Add index for ProcessedByUserId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AccessRequests_ProcessedByUserId' AND object_id = OBJECT_ID(N'[dbo].[AccessRequests]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AccessRequests_ProcessedByUserId]
    ON [AccessRequests] ([ProcessedByUserId]);
    PRINT 'Created index IX_AccessRequests_ProcessedByUserId';
END
GO

-- Add foreign key for ProcessedByUserId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AccessRequests_Users_ProcessedByUserId')
BEGIN
    ALTER TABLE [AccessRequests]
    ADD CONSTRAINT [FK_AccessRequests_Users_ProcessedByUserId]
    FOREIGN KEY ([ProcessedByUserId])
    REFERENCES [Users] ([Id])
    ON DELETE NO ACTION;
    PRINT 'Added FK_AccessRequests_Users_ProcessedByUserId foreign key';
END
GO

-- =========================================================
-- STEP 2: Create HttpAuditLogs table
-- =========================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]') AND type = 'U')
BEGIN
    CREATE TABLE [HttpAuditLogs] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] bigint NULL,
        [Method] nvarchar(10) NOT NULL,
        [Path] nvarchar(500) NOT NULL,
        [QueryString] nvarchar(2000) NULL,
        [StatusCode] int NOT NULL,
        [IpAddress] nvarchar(50) NOT NULL,
        [UserAgent] nvarchar(500) NULL,
        [DurationMs] bigint NOT NULL,
        [Timestamp] datetime2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Username] nvarchar(100) NULL,
        [AdditionalInfo] nvarchar(2000) NULL,
        CONSTRAINT [PK_HttpAuditLogs] PRIMARY KEY ([Id])
    );
    PRINT 'Created HttpAuditLogs table';
END
GO

-- Add foreign key for UserId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_HttpAuditLogs_Users_UserId')
BEGIN
    ALTER TABLE [HttpAuditLogs]
    ADD CONSTRAINT [FK_HttpAuditLogs_Users_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [Users] ([Id])
    ON DELETE NO ACTION;
    PRINT 'Added FK_HttpAuditLogs_Users_UserId foreign key';
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HttpAuditLogs_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HttpAuditLogs_Timestamp]
    ON [HttpAuditLogs] ([Timestamp]);
    PRINT 'Created index IX_HttpAuditLogs_Timestamp';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HttpAuditLogs_UserId' AND object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HttpAuditLogs_UserId]
    ON [HttpAuditLogs] ([UserId]);
    PRINT 'Created index IX_HttpAuditLogs_UserId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HttpAuditLogs_StatusCode' AND object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HttpAuditLogs_StatusCode]
    ON [HttpAuditLogs] ([StatusCode]);
    PRINT 'Created index IX_HttpAuditLogs_StatusCode';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HttpAuditLogs_Method_Path' AND object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HttpAuditLogs_Method_Path]
    ON [HttpAuditLogs] ([Method], [Path]);
    PRINT 'Created index IX_HttpAuditLogs_Method_Path';
END
GO

-- =========================================================
-- STEP 3: Update migration history (IMPORTANT!)
-- =========================================================
-- Add this migration to the history so EF Core knows it's applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251129140000_AddAccessRequestsAndHttpAuditLogs')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251129140000_AddAccessRequestsAndHttpAuditLogs', '8.0.0');
    PRINT 'Added migration to __EFMigrationsHistory';
END
GO

PRINT 'Migration completed successfully!';
GO

-- Verify the changes
SELECT 
    CASE 
        WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedByUserId') THEN '?'
        ELSE '?'
    END AS 'ProcessedByUserId',
    CASE 
        WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ProcessedAt') THEN '?'
        ELSE '?'
    END AS 'ProcessedAt',
    CASE 
        WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'RequestedDurationHours') THEN '?'
        ELSE '?'
    END AS 'RequestedDurationHours',
    CASE 
        WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AccessRequests]') AND name = 'ReviewerComment') THEN '?'
        ELSE '?'
    END AS 'ReviewerComment',
    CASE 
        WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HttpAuditLogs]') AND type = 'U') THEN '?'
        ELSE '?'
    END AS 'HttpAuditLogs_Table';
GO
