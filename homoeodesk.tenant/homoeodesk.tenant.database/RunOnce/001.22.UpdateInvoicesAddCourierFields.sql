-- ====================================
-- HomoeoDesk Tenant Database - Update Invoices Table
-- ====================================
-- Add missing columns for courier management and prescription link
-- ====================================

USE HomoeoDesk_demo; -- Replace with actual tenant database name
GO

-- Add PrescriptionId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'PrescriptionId')
BEGIN
    ALTER TABLE Invoices
    ADD PrescriptionId INT NULL;
    
    ALTER TABLE Invoices
    ADD CONSTRAINT FK_Invoices_Prescription FOREIGN KEY (PrescriptionId) 
        REFERENCES Prescriptions(Id) ON DELETE NO ACTION;
    
    PRINT 'Added PrescriptionId column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'PrescriptionId column already exists in Invoices table.';
END
GO

-- Add CourierDocketNumber column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CourierDocketNumber')
BEGIN
    ALTER TABLE Invoices
    ADD CourierDocketNumber NVARCHAR(100) NULL;
    
    PRINT 'Added CourierDocketNumber column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'CourierDocketNumber column already exists in Invoices table.';
END
GO

-- Add CourierCompany column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CourierCompany')
BEGIN
    ALTER TABLE Invoices
    ADD CourierCompany NVARCHAR(200) NULL;
    
    PRINT 'Added CourierCompany column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'CourierCompany column already exists in Invoices table.';
END
GO

-- Add CourierDispatchedDate column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CourierDispatchedDate')
BEGIN
    ALTER TABLE Invoices
    ADD CourierDispatchedDate DATETIME2 NULL;
    
    PRINT 'Added CourierDispatchedDate column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'CourierDispatchedDate column already exists in Invoices table.';
END
GO

-- Add CourierTrackingUrl column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CourierTrackingUrl')
BEGIN
    ALTER TABLE Invoices
    ADD CourierTrackingUrl NVARCHAR(500) NULL;
    
    PRINT 'Added CourierTrackingUrl column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'CourierTrackingUrl column already exists in Invoices table.';
END
GO

-- Add CourierStatus column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CourierStatus')
BEGIN
    ALTER TABLE Invoices
    ADD CourierStatus INT NULL; -- 0=NotDispatched, 1=Dispatched, 2=InTransit, 3=OutForDelivery, 4=Delivered, 5=Returned
    
    PRINT 'Added CourierStatus column to Invoices table.';
END
ELSE
BEGIN
    PRINT 'CourierStatus column already exists in Invoices table.';
END
GO

PRINT 'Invoices table update completed.';
GO


