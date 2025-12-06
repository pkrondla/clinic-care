-- ====================================
-- Update Consultations Table Schema to Match Entity Model
-- ====================================
-- The database schema doesn't match the entity model
-- This script updates the table to match the Consultation entity
-- ====================================

USE ClinicCare_demo;
GO

-- Check if table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultations')
BEGIN
    PRINT 'Updating Consultations table schema...';
    
    -- Add missing columns
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'OrganizationId')
    BEGIN
        ALTER TABLE Consultations ADD OrganizationId INT NOT NULL DEFAULT 1;
        PRINT 'Added OrganizationId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'DoctorId')
    BEGIN
        ALTER TABLE Consultations ADD DoctorId INT NOT NULL DEFAULT 1;
        PRINT 'Added DoctorId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'PatientId')
    BEGIN
        ALTER TABLE Consultations ADD PatientId INT NOT NULL DEFAULT 1;
        PRINT 'Added PatientId column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'Examination')
    BEGIN
        -- Rename Observations to Examination if it exists, otherwise add it
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'Observations')
        BEGIN
            EXEC sp_rename 'Consultations.Observations', 'Examination', 'COLUMN';
            PRINT 'Renamed Observations to Examination.';
        END
        ELSE
        BEGIN
            ALTER TABLE Consultations ADD Examination NVARCHAR(MAX) NULL;
            PRINT 'Added Examination column.';
        END
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'Notes')
    BEGIN
        -- Rename DoctorNotes to Notes if it exists, otherwise add it
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'DoctorNotes')
        BEGIN
            EXEC sp_rename 'Consultations.DoctorNotes', 'Notes', 'COLUMN';
            PRINT 'Renamed DoctorNotes to Notes.';
        END
        ELSE
        BEGIN
            ALTER TABLE Consultations ADD Notes NVARCHAR(MAX) NULL;
            PRINT 'Added Notes column.';
        END
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'ConsultationFee')
    BEGIN
        ALTER TABLE Consultations ADD ConsultationFee DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT 'Added ConsultationFee column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'ConsultationDate')
    BEGIN
        ALTER TABLE Consultations ADD ConsultationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added ConsultationDate column.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'IsActive')
    BEGIN
        ALTER TABLE Consultations ADD IsActive BIT NOT NULL DEFAULT 1;
        PRINT 'Added IsActive column.';
    END
    
    -- Remove obsolete columns (optional - comment out if you want to keep them)
    -- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'VitalSigns')
    -- BEGIN
    --     ALTER TABLE Consultations DROP COLUMN VitalSigns;
    --     PRINT 'Removed VitalSigns column.';
    -- END
    
    -- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'Duration')
    -- BEGIN
    --     ALTER TABLE Consultations DROP COLUMN Duration;
    --     PRINT 'Removed Duration column.';
    -- END
    
    -- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'ConsultationType')
    -- BEGIN
    --     ALTER TABLE Consultations DROP COLUMN ConsultationType;
    --     PRINT 'Removed ConsultationType column.';
    -- END
    
    -- Update foreign key constraints
    -- Drop existing foreign key if it references Users
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultations_Doctor' AND parent_object_id = OBJECT_ID('Consultations'))
    BEGIN
        ALTER TABLE Consultations DROP CONSTRAINT FK_Consultations_Doctor;
        PRINT 'Dropped FK_Consultations_Doctor constraint.';
    END
    
    -- Note: OrganizationId foreign key is not added here because Organizations table is in the global database
    -- The foreign key relationship is handled at the application level
    
    -- Add foreign key for DoctorId (to DoctorProfiles)
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultations_DoctorProfile')
    BEGIN
        ALTER TABLE Consultations
        ADD CONSTRAINT FK_Consultations_DoctorProfile
        FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id)
        ON DELETE NO ACTION;
        PRINT 'Added FK_Consultations_DoctorProfile constraint.';
    END
    
    -- Add foreign key for PatientId
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultations_Patient')
    BEGIN
        ALTER TABLE Consultations
        ADD CONSTRAINT FK_Consultations_Patient
        FOREIGN KEY (PatientId) REFERENCES Patients(Id)
        ON DELETE NO ACTION;
        PRINT 'Added FK_Consultations_Patient constraint.';
    END
    
    PRINT 'Consultations table schema updated successfully.';
END
ELSE
BEGIN
    PRINT 'Consultations table does not exist.';
END
GO

