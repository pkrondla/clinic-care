-- ====================================
-- ClinicCare Tenant Database - Create SmsSettings Table
-- ====================================
-- This script creates the SmsSettings table for storing
-- organization-level SMS provider configuration

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SmsSettings')
BEGIN
    CREATE TABLE SmsSettings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OrganizationId INT NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        Provider NVARCHAR(50) NULL,
        ApiKey NVARCHAR(500) NULL,
        ApiSecret NVARCHAR(500) NULL,
        AccountSid NVARCHAR(100) NULL,
        AuthToken NVARCHAR(500) NULL,
        FromPhoneNumber NVARCHAR(50) NULL,
        SenderId NVARCHAR(50) NULL,
        ApiUrl NVARCHAR(500) NULL,
        TimeoutSeconds INT NULL DEFAULT 30,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
    );
    
    -- Unique constraint: One settings record per organization
    CREATE UNIQUE INDEX IX_SmsSettings_OrganizationId 
        ON SmsSettings(OrganizationId);
    
    PRINT 'Created SmsSettings table.';
END
ELSE
BEGIN
    PRINT 'SmsSettings table already exists.';
END
GO

