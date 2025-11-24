-- ====================================
-- ClinicCare Tenant Database - Users Table
-- ====================================
-- This table stores organization users (admin, doctors, staff)
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Role INT NOT NULL, -- 1=OrganizationAdmin, 2=Doctor, 3=Reception, 4=Pharmacy, 5=Patient
        SelectedClinicId INT NULL,
        RefreshToken NVARCHAR(MAX) NULL,
        RefreshTokenExpiryTime DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT UK_Users_Email UNIQUE (Email)
    );
    PRINT 'Table Users created.';
END
ELSE
BEGIN
    PRINT 'Table Users already exists.';
END
GO

