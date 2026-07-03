-- ====================================
-- HomoeoDesk Tenant Database - Stock Transactions Table
-- ====================================
-- This table stores stock movement transactions
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockTransactions')
BEGIN
    CREATE TABLE StockTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ClinicId INT NOT NULL,
        MedicineId INT NOT NULL,
        TransactionType INT NOT NULL, -- 1=Purchase, 2=Sale, 3=Transfer, 4=Adjustment
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        Reference NVARCHAR(100) NULL,
        Notes NVARCHAR(500) NULL,
        FromClinicId INT NULL,
        ToClinicId INT NULL,
        TransactionDate DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_StockTransactions_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
        CONSTRAINT FK_StockTransactions_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id),
        CONSTRAINT FK_StockTransactions_FromClinic FOREIGN KEY (FromClinicId) REFERENCES Clinics(Id),
        CONSTRAINT FK_StockTransactions_ToClinic FOREIGN KEY (ToClinicId) REFERENCES Clinics(Id)
    );
    PRINT 'Table StockTransactions created.';
END
ELSE
BEGIN
    PRINT 'Table StockTransactions already exists.';
END
GO


