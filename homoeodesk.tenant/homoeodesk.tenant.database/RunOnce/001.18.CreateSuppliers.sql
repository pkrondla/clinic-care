-- ====================================
-- HomoeoDesk Tenant Database - Suppliers Table
-- ====================================
-- This table stores supplier information for inventory management
-- ====================================

USE HomoeoDesk_demo; -- Replace with actual tenant database name
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Suppliers')
BEGIN
    CREATE TABLE Suppliers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        ContactPerson NVARCHAR(100) NULL,
        Email NVARCHAR(255) NULL,
        Phone NVARCHAR(20) NULL,
        AlternatePhone NVARCHAR(20) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        PinCode NVARCHAR(10) NULL,
        GSTNumber NVARCHAR(15) NULL,
        PANNumber NVARCHAR(10) NULL,
        BankName NVARCHAR(200) NULL,
        BankAccountNumber NVARCHAR(50) NULL,
        IFSCCode NVARCHAR(11) NULL,
        Notes NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_Suppliers_OrganizationName ON Suppliers(OrganizationId, Name);
    
    PRINT 'Table Suppliers created.';
END
ELSE
BEGIN
    PRINT 'Table Suppliers already exists.';
END
GO


