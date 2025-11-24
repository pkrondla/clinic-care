-- ====================================
-- ClinicCare Tenant Database - Update UserClinicAccess Table
-- ====================================
-- This script adds the missing IsActive and UpdatedAt columns to UserClinicAccess table
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

PRINT 'Updating UserClinicAccess table...';

-- Add IsActive column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserClinicAccess') AND name = 'IsActive')
BEGIN
    ALTER TABLE UserClinicAccess ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT 'Added IsActive column.';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists.';
END

-- Add UpdatedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserClinicAccess') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE UserClinicAccess ADD UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Added UpdatedAt column.';
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already exists.';
END

-- Verify the update
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'UserClinicAccess'
ORDER BY ORDINAL_POSITION;

PRINT 'UserClinicAccess table update completed.';
GO

