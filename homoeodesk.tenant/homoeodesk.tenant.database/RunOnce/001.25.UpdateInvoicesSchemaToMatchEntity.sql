-- ====================================
-- HomoeoDesk Tenant Database - Update Invoices Table Schema
-- ====================================
-- Update Invoices table to match the entity model
-- ====================================

USE HomoeoDesk_demo; -- Replace with actual tenant database name
GO

-- Add missing columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE Invoices
    ADD OrganizationId INT NOT NULL DEFAULT 1; -- Set default for existing rows
    
    PRINT 'Added OrganizationId column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'IsActive')
BEGIN
    ALTER TABLE Invoices
    ADD IsActive BIT NOT NULL DEFAULT 1;
    
    PRINT 'Added IsActive column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'ConsultationId')
BEGIN
    ALTER TABLE Invoices
    ADD ConsultationId INT NULL;
    
    ALTER TABLE Invoices
    ADD CONSTRAINT FK_Invoices_Consultation FOREIGN KEY (ConsultationId) 
        REFERENCES Consultations(Id) ON DELETE NO ACTION;
    
    PRINT 'Added ConsultationId column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'InvoiceDate')
BEGIN
    ALTER TABLE Invoices
    ADD InvoiceDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    
    PRINT 'Added InvoiceDate column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'PaymentReference')
BEGIN
    ALTER TABLE Invoices
    ADD PaymentReference NVARCHAR(100) NULL;
    
    PRINT 'Added PaymentReference column to Invoices table.';
END
GO

-- Rename columns to match entity model
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'ConsultationCharges')
BEGIN
    EXEC sp_rename 'Invoices.ConsultationCharges', 'ConsultationAmount', 'COLUMN';
    PRINT 'Renamed ConsultationCharges to ConsultationAmount.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'MedicineCharges')
BEGIN
    EXEC sp_rename 'Invoices.MedicineCharges', 'MedicineAmount', 'COLUMN';
    PRINT 'Renamed MedicineCharges to MedicineAmount.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'PaymentStatus')
BEGIN
    EXEC sp_rename 'Invoices.PaymentStatus', 'Status', 'COLUMN';
    PRINT 'Renamed PaymentStatus to Status.';
END
GO

-- Add PaidAmount and BalanceAmount if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'PaidAmount')
BEGIN
    ALTER TABLE Invoices
    ADD PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    
    -- Calculate PaidAmount from FinalAmount if FinalAmount exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'FinalAmount')
    BEGIN
        UPDATE Invoices
        SET PaidAmount = CASE WHEN PaymentStatus = 2 THEN FinalAmount ELSE 0 END;
    END
    
    PRINT 'Added PaidAmount column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'BalanceAmount')
BEGIN
    ALTER TABLE Invoices
    ADD BalanceAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    
    -- Calculate BalanceAmount from FinalAmount and PaidAmount
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'FinalAmount')
    BEGIN
        UPDATE Invoices
        SET BalanceAmount = FinalAmount - PaidAmount;
    END
    
    PRINT 'Added BalanceAmount column to Invoices table.';
END
GO

-- Remove AppointmentId if it exists (we use ConsultationId instead)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'AppointmentId')
BEGIN
    -- Drop foreign key if it exists
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoices_Appointment')
    BEGIN
        ALTER TABLE Invoices DROP CONSTRAINT FK_Invoices_Appointment;
    END
    
    ALTER TABLE Invoices DROP COLUMN AppointmentId;
    PRINT 'Removed AppointmentId column from Invoices table.';
END
GO

-- Remove columns that are not in the entity model
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Discount')
BEGIN
    ALTER TABLE Invoices DROP COLUMN Discount;
    PRINT 'Removed Discount column from Invoices table.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'TaxAmount')
BEGIN
    ALTER TABLE Invoices DROP COLUMN TaxAmount;
    PRINT 'Removed TaxAmount column from Invoices table.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'FinalAmount')
BEGIN
    ALTER TABLE Invoices DROP COLUMN FinalAmount;
    PRINT 'Removed FinalAmount column from Invoices table.';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Notes')
BEGIN
    ALTER TABLE Invoices DROP COLUMN Notes;
    PRINT 'Removed Notes column from Invoices table.';
END
GO

PRINT 'Invoices table schema update completed.';
GO


