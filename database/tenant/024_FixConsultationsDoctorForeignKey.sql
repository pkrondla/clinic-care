-- ====================================
-- Fix Consultations.DoctorId Foreign Key
-- ====================================
-- The database has DoctorId referencing Users(Id), but it should reference DoctorProfiles(Id)
-- This script updates the foreign key to match the entity model
-- ====================================

USE ClinicCare_demo;
GO

-- Drop the existing foreign key
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultations_Doctor')
BEGIN
    ALTER TABLE Consultations
    DROP CONSTRAINT FK_Consultations_Doctor;
    PRINT 'Dropped FK_Consultations_Doctor constraint.';
END
GO

-- Add the correct foreign key referencing DoctorProfiles
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultations_DoctorProfile')
BEGIN
    ALTER TABLE Consultations
    ADD CONSTRAINT FK_Consultations_DoctorProfile
    FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id)
    ON DELETE NO ACTION;
    PRINT 'Added FK_Consultations_DoctorProfile constraint.';
END
GO

