-- ====================================
-- HomoeoDesk Tenant Database - Prescription Items Table
-- ====================================
-- This table stores prescription line items (medicines)
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
BEGIN
    CREATE TABLE PrescriptionItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        PrescriptionId INT NOT NULL,
        MedicineId INT NOT NULL,
        MedicineName NVARCHAR(200) NOT NULL,
        Dosage NVARCHAR(100) NOT NULL,
        Frequency NVARCHAR(100) NOT NULL,
        Duration NVARCHAR(100) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        TotalPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
        Instructions NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_PrescriptionItems_Prescription FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id),
        CONSTRAINT FK_PrescriptionItems_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id)
    );
    PRINT 'Table PrescriptionItems created.';
END
ELSE
BEGIN
    PRINT 'Table PrescriptionItems already exists.';
END
GO


