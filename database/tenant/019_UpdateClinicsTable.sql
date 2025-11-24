-- ====================================
-- ClinicCare Tenant Database - Update Clinics Table
-- ====================================
-- This script adds the missing OrganizationId column to the Clinics table
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

PRINT 'Updating Clinics table...';

-- Add OrganizationId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clinics') AND name = 'OrganizationId')
BEGIN
    -- Add column with default value (this will automatically set all existing rows to 1)
    ALTER TABLE Clinics ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column with default value 1.';
    PRINT 'All existing clinic records have been set to OrganizationId = 1.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END

-- Verify the update
SELECT 
    c.Id,
    c.Name,
    c.Code,
    c.OrganizationId,
    c.IsActive
FROM Clinics c
ORDER BY c.Id;

PRINT 'Clinics table update completed.';
GO

