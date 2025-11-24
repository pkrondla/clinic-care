-- ====================================
-- ClinicCare Global Database - Organizations Table
-- ====================================
-- This table stores tenant organizations
-- ====================================

USE ClinicCare_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Organizations')
BEGIN
    CREATE TABLE Organizations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Subdomain NVARCHAR(100) NOT NULL,
        ContactEmail NVARCHAR(255) NOT NULL,
        ContactPhone NVARCHAR(20) NULL,
        Address NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT UK_Organizations_Subdomain UNIQUE (Subdomain)
    );
    PRINT 'Table Organizations created.';
END
ELSE
BEGIN
    PRINT 'Table Organizations already exists.';
END
GO

