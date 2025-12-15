-- ====================================
-- ClinicCare Tenant Database - Add PhotoUrl to Patients Table
-- ====================================
-- This script adds the PhotoUrl column to the Patients table
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'PhotoUrl')
BEGIN
    ALTER TABLE Patients ADD PhotoUrl NVARCHAR(MAX) NULL;
    PRINT 'Added PhotoUrl column to Patients table.';
END
ELSE
BEGIN
    PRINT 'PhotoUrl column already exists in Patients table.';
END
GO

