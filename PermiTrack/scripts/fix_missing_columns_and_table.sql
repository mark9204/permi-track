-- Create HttpAuditLogs table if missing
IF OBJECT_ID('dbo.HttpAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HttpAuditLogs
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        AdditionalInfo NVARCHAR(2000) NULL,
        DurationMs INT NULL,
        IpAddress NVARCHAR(50) NULL,
        Method NVARCHAR(10) NULL,
        Path NVARCHAR(500) NULL,
        QueryString NVARCHAR(2000) NULL,
        StatusCode INT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UserAgent NVARCHAR(500) NULL,
        UserId BIGINT NULL,
        Username NVARCHAR(100) NULL
    );
    PRINT 'Created dbo.HttpAuditLogs';
END
ELSE
    PRINT 'dbo.HttpAuditLogs already exists';

-- Add ParentRoleId to Roles if missing
IF COL_LENGTH('dbo.Roles', 'ParentRoleId') IS NULL
BEGIN
    ALTER TABLE dbo.Roles
    ADD ParentRoleId BIGINT NULL;

    -- Optional FK if Users/Roles schema supports it. Remove if not desired:
    -- ALTER TABLE dbo.Roles ADD CONSTRAINT FK_Roles_ParentRole FOREIGN KEY (ParentRoleId) REFERENCES dbo.Roles(Id);
    PRINT 'Added Roles.ParentRoleId';
END
ELSE
    PRINT 'Roles.ParentRoleId already exists';

-- Add AccessRequests columns if missing
IF COL_LENGTH('dbo.AccessRequests', 'ProcessedAt') IS NULL
BEGIN
    ALTER TABLE dbo.AccessRequests
    ADD ProcessedAt DATETIME2 NULL;
    PRINT 'Added AccessRequests.ProcessedAt';
END
ELSE
    PRINT 'AccessRequests.ProcessedAt already exists';

IF COL_LENGTH('dbo.AccessRequests', 'ProcessedByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.AccessRequests
    ADD ProcessedByUserId BIGINT NULL;
    PRINT 'Added AccessRequests.ProcessedByUserId';
END
ELSE
    PRINT 'AccessRequests.ProcessedByUserId already exists';

IF COL_LENGTH('dbo.AccessRequests', 'RequestedDurationHours') IS NULL
BEGIN
    ALTER TABLE dbo.AccessRequests
    ADD RequestedDurationHours INT NULL;
    PRINT 'Added AccessRequests.RequestedDurationHours';
END
ELSE
    PRINT 'AccessRequests.RequestedDurationHours already exists';

IF COL_LENGTH('dbo.AccessRequests', 'ReviewerComment') IS NULL
BEGIN
    ALTER TABLE dbo.AccessRequests
    ADD ReviewerComment NVARCHAR(2000) NULL;
    PRINT 'Added AccessRequests.ReviewerComment';
END
ELSE
    PRINT 'AccessRequests.ReviewerComment already exists';