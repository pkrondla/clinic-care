-- ====================================
-- ClinicCare Global Database - Global Medicines Table
-- ====================================
-- This table stores the shared homoeopathic medicine catalog
-- ====================================

USE ClinicCare_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GlobalMedicines')
BEGIN
    CREATE TABLE GlobalMedicines (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        GenericName NVARCHAR(200) NOT NULL,
        Type NVARCHAR(100) NOT NULL, -- Dilution, Tablet, Mother Tincture, etc.
        Potency NVARCHAR(50) NOT NULL, -- 30C, 200C, Q, etc.
        Manufacturer NVARCHAR(200) NOT NULL,
        Price DECIMAL(10,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table GlobalMedicines created.';
END
ELSE
BEGIN
    PRINT 'Table GlobalMedicines already exists.';
END
GO

