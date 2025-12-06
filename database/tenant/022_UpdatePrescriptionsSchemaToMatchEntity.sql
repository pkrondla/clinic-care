-- ====================================
-- ClinicCare Tenant Database - Update Prescriptions Table Schema
-- ====================================
-- Update Prescriptions table to match the entity model
-- ====================================

USE ClinicCare_demo; -- Replace with actual tenant database name
GO

-- Add missing columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE Prescriptions
    ADD OrganizationId INT NOT NULL DEFAULT 1; -- Set default for existing rows
    
    PRINT 'Added OrganizationId column to Prescriptions table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'IsActive')
BEGIN
    ALTER TABLE Prescriptions
    ADD IsActive BIT NOT NULL DEFAULT 1;
    
    PRINT 'Added IsActive column to Prescriptions table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'Status')
BEGIN
    ALTER TABLE Prescriptions
    ADD Status INT NOT NULL DEFAULT 1; -- 1=Draft, 2=Issued, 3=Dispensed
    
    PRINT 'Added Status column to Prescriptions table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'IssuedDate')
BEGIN
    ALTER TABLE Prescriptions
    ADD IssuedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    
    PRINT 'Added IssuedDate column to Prescriptions table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'InternalNotes')
BEGIN
    ALTER TABLE Prescriptions
    ADD InternalNotes NVARCHAR(MAX) NULL;
    
    PRINT 'Added InternalNotes column to Prescriptions table.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'PatientInstructions')
BEGIN
    ALTER TABLE Prescriptions
    ADD PatientInstructions NVARCHAR(MAX) NULL;
    
    -- Migrate data from Instructions if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Prescriptions') AND name = 'Instructions')
    BEGIN
        UPDATE Prescriptions
        SET PatientInstructions = Instructions
        WHERE PatientInstructions IS NULL AND Instructions IS NOT NULL;
    END
    
    PRINT 'Added PatientInstructions column to Prescriptions table.';
END
GO

-- Remove columns that are not in the entity model (keep them for now, just mark as deprecated)
-- We'll keep Instructions, DietAdvice, LifestyleAdvice, FollowupDate, FollowupNotes for backward compatibility
-- but they won't be used by the entity model

PRINT 'Prescriptions table schema update completed.';
GO

