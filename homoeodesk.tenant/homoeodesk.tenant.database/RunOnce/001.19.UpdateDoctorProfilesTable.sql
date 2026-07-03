-- ====================================
-- HomoeoDesk Tenant Database - Update DoctorProfiles Table
-- ====================================
-- Add missing columns to DoctorProfiles table if they don't exist
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

PRINT 'Updating DoctorProfiles table...';
GO

-- Add OrganizationId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DoctorProfiles') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE DoctorProfiles ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END
GO

-- Add IsActive if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DoctorProfiles') AND name = 'IsActive')
BEGIN
    ALTER TABLE DoctorProfiles ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT 'Added IsActive column.';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists.';
END
GO

PRINT 'DoctorProfiles table update completed.';
GO



