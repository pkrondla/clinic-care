-- ====================================
-- ClinicCare Tenant Database - Create EmailSettings Table
-- ====================================
-- This script creates the EmailSettings table for storing
-- organization-level SMTP email configuration

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailSettings')
BEGIN
    CREATE TABLE EmailSettings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OrganizationId INT NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        SmtpServer NVARCHAR(255) NULL,
        SmtpPort INT NULL DEFAULT 587,
        UseSsl BIT NOT NULL DEFAULT 1,
        UseTls BIT NOT NULL DEFAULT 1,
        SmtpUsername NVARCHAR(255) NULL,
        SmtpPassword NVARCHAR(1000) NULL,
        FromEmail NVARCHAR(255) NULL,
        FromName NVARCHAR(255) NULL,
        ReplyToEmail NVARCHAR(255) NULL,
        TimeoutSeconds INT NULL DEFAULT 30,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
    );
    
    -- Unique constraint: One settings record per organization
    CREATE UNIQUE INDEX IX_EmailSettings_OrganizationId 
        ON EmailSettings(OrganizationId);
    
    PRINT 'Created EmailSettings table.';
END
ELSE
BEGIN
    PRINT 'EmailSettings table already exists.';
END
GO

