-- ====================================
-- HomoeoDesk Tenant Database - Clinics Table
-- ====================================
-- This table stores physical clinic locations
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clinics')
BEGIN
    CREATE TABLE Clinics (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Address NVARCHAR(500) NULL,
        ContactPhone NVARCHAR(20) NULL,
        ContactEmail NVARCHAR(255) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT UK_Clinics_Code UNIQUE (Code)
    );
    PRINT 'Table Clinics created.';
END
ELSE
BEGIN
    PRINT 'Table Clinics already exists.';
END
GO


