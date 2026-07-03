-- ====================================
-- HomoeoDesk Tenant Database - Clinic Medicines Table
-- ====================================
-- This table stores clinic-specific medicine catalog
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClinicMedicines')
BEGIN
    CREATE TABLE ClinicMedicines (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ClinicId INT NOT NULL,
        GlobalMedicineId INT NULL, -- Reference to global database
        Name NVARCHAR(200) NOT NULL,
        GenericName NVARCHAR(200) NOT NULL,
        Type NVARCHAR(100) NOT NULL,
        Potency NVARCHAR(50) NOT NULL,
        Manufacturer NVARCHAR(200) NOT NULL,
        PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_ClinicMedicines_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id)
    );
    PRINT 'Table ClinicMedicines created.';
END
ELSE
BEGIN
    PRINT 'Table ClinicMedicines already exists.';
END
GO


