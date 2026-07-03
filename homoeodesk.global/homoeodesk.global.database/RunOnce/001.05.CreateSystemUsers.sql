-- ====================================
-- HomoeoDesk Global Database - System Users Table
-- ====================================
-- This table stores super admin and system admin accounts
-- ====================================

USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemUsers')
BEGIN
    CREATE TABLE SystemUsers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Role INT NOT NULL DEFAULT 1, -- 1=SuperAdmin, 2=SystemAdmin
        IsActive BIT NOT NULL DEFAULT 1,
        LastLoginAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT UK_SystemUsers_Email UNIQUE (Email)
    );
    PRINT 'Table SystemUsers created.';
END
ELSE
BEGIN
    PRINT 'Table SystemUsers already exists.';
END
GO


