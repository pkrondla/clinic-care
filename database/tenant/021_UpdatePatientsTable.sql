-- ====================================
-- ClinicCare Tenant Database - Update Patients Table
-- ====================================
-- This script adds the missing OrganizationId, UserId, and EmergencyContact columns
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

PRINT 'Updating Patients table...';

-- Add OrganizationId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE Patients ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END

-- Add UserId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'UserId')
BEGIN
    ALTER TABLE Patients ADD UserId INT NULL;
    PRINT 'Added UserId column.';
END
ELSE
BEGIN
    PRINT 'UserId column already exists.';
END

-- Add EmergencyContact column if it doesn't exist
-- Note: The table has EmergencyContactName and EmergencyContactPhone, but the entity expects EmergencyContact
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Patients') AND name = 'EmergencyContact')
BEGIN
    ALTER TABLE Patients ADD EmergencyContact NVARCHAR(500) NULL;
    PRINT 'Added EmergencyContact column.';
    
    -- Populate EmergencyContact from existing EmergencyContactName and EmergencyContactPhone if available
    UPDATE Patients 
    SET EmergencyContact = CASE 
        WHEN EmergencyContactName IS NOT NULL AND EmergencyContactPhone IS NOT NULL 
            THEN EmergencyContactName + ' - ' + EmergencyContactPhone
        WHEN EmergencyContactName IS NOT NULL 
            THEN EmergencyContactName
        WHEN EmergencyContactPhone IS NOT NULL 
            THEN EmergencyContactPhone
        ELSE NULL
    END
    WHERE EmergencyContact IS NULL;
    PRINT 'Populated EmergencyContact from existing data.';
END
ELSE
BEGIN
    PRINT 'EmergencyContact column already exists.';
END

-- Verify the update
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Patients'
    AND COLUMN_NAME IN ('OrganizationId', 'UserId', 'EmergencyContact', 'IsActive')
ORDER BY COLUMN_NAME;

PRINT 'Patients table update completed.';
GO

