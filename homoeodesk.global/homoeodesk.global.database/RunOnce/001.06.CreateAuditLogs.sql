-- ====================================
-- HomoeoDesk Global Database - Audit Logs Table
-- ====================================
-- This table stores system-wide audit trail
-- ====================================

USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NULL, -- NULL for system-level actions
        UserId INT NULL,
        UserEmail NVARCHAR(255) NULL,
        Action NVARCHAR(100) NOT NULL, -- Created, Updated, Deleted, Login, etc.
        EntityType NVARCHAR(100) NOT NULL, -- Organization, User, Medicine, etc.
        EntityId INT NULL,
        Details NVARCHAR(MAX) NULL, -- JSON with changes
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table AuditLogs created.';
END
ELSE
BEGIN
    PRINT 'Table AuditLogs already exists.';
END
GO


