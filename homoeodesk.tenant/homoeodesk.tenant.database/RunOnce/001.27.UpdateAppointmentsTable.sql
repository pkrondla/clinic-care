-- ====================================
-- HomoeoDesk Tenant Database - Update Appointments Table
-- ====================================
-- This script adds the missing OrganizationId and IsActive columns
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

PRINT 'Updating Appointments table...';

-- Add OrganizationId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE Appointments ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END

-- Add IsActive column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'IsActive')
BEGIN
    ALTER TABLE Appointments ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT 'Added IsActive column.';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists.';
END

-- Verify the update
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Appointments'
    AND COLUMN_NAME IN ('OrganizationId', 'IsActive')
ORDER BY COLUMN_NAME;

PRINT 'Appointments table update completed.';
GO


