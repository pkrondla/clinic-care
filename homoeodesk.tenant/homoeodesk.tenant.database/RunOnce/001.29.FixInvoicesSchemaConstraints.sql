-- ====================================
-- HomoeoDesk Tenant Database - Fix Invoices Table Constraints
-- ====================================
-- Drop constraints and indexes before removing columns
-- ====================================

USE HomoeoDesk_demo;
GO

-- Drop index on AppointmentId if it exists
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_Appointment' AND object_id = OBJECT_ID('Invoices'))
BEGIN
    DROP INDEX IX_Invoices_Appointment ON Invoices;
    PRINT 'Dropped IX_Invoices_Appointment index.';
END
GO

-- Drop foreign key on AppointmentId if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoices_Appointment' AND parent_object_id = OBJECT_ID('Invoices'))
BEGIN
    ALTER TABLE Invoices DROP CONSTRAINT FK_Invoices_Appointment;
    PRINT 'Dropped FK_Invoices_Appointment foreign key.';
END
GO

-- Drop default constraints
IF EXISTS (SELECT * FROM sys.default_constraints WHERE name LIKE 'DF__Invoices__Discou%' AND parent_object_id = OBJECT_ID('Invoices'))
BEGIN
    DECLARE @constraintName NVARCHAR(200);
    SELECT @constraintName = name FROM sys.default_constraints WHERE name LIKE 'DF__Invoices__Discou%' AND parent_object_id = OBJECT_ID('Invoices');
    EXEC('ALTER TABLE Invoices DROP CONSTRAINT ' + @constraintName);
    PRINT 'Dropped Discount default constraint.';
END
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name LIKE 'DF__Invoices__TaxAmo%' AND parent_object_id = OBJECT_ID('Invoices'))
BEGIN
    DECLARE @constraintName2 NVARCHAR(200);
    SELECT @constraintName2 = name FROM sys.default_constraints WHERE name LIKE 'DF__Invoices__TaxAmo%' AND parent_object_id = OBJECT_ID('Invoices');
    EXEC('ALTER TABLE Invoices DROP CONSTRAINT ' + @constraintName2);
    PRINT 'Dropped TaxAmount default constraint.';
END
GO

-- Now add PaidAmount and BalanceAmount
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'PaidAmount')
BEGIN
    ALTER TABLE Invoices
    ADD PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    
    -- Calculate PaidAmount from Status (if Status = 2, it's paid)
    UPDATE Invoices
    SET PaidAmount = CASE WHEN Status = 2 THEN TotalAmount ELSE 0 END;
    
    PRINT 'Added PaidAmount column to Invoices table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'BalanceAmount')
BEGIN
    ALTER TABLE Invoices
    ADD BalanceAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    
    -- Calculate BalanceAmount
    UPDATE Invoices
    SET BalanceAmount = TotalAmount - PaidAmount;
    
    PRINT 'Added BalanceAmount column to Invoices table.';
END
GO

-- Now remove columns
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'AppointmentId')
BEGIN
    ALTER TABLE Invoices DROP COLUMN AppointmentId;
    PRINT 'Removed AppointmentId column from Invoices table.';
END
GO

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

PRINT 'Invoices table constraints fixed.';
GO


