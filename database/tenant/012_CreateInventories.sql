-- ====================================
-- ClinicCare Tenant Database - Inventories Table
-- ====================================
-- This table stores per-clinic stock management
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventories')
BEGIN
    CREATE TABLE Inventories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ClinicId INT NOT NULL,
        MedicineId INT NOT NULL,
        CurrentStock INT NOT NULL DEFAULT 0,
        MinimumStock INT NOT NULL DEFAULT 0,
        MaximumStock INT NOT NULL DEFAULT 0,
        PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        ExpiryDate DATE NOT NULL,
        BatchNumber NVARCHAR(50) NULL,
        LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Inventories_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
        CONSTRAINT FK_Inventories_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id)
    );
    PRINT 'Table Inventories created.';
END
ELSE
BEGIN
    PRINT 'Table Inventories already exists.';
END
GO

