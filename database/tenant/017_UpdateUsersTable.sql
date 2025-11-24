-- ====================================
-- ClinicCare Tenant Database - Update Users Table
-- ====================================
-- Add missing columns to Users table if they don't exist
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

PRINT 'Updating Users table...';
GO

-- Add OrganizationId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'OrganizationId')
BEGIN
    ALTER TABLE Users ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END
GO

-- Add RefreshToken if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RefreshToken')
BEGIN
    ALTER TABLE Users ADD RefreshToken NVARCHAR(MAX) NULL;
    PRINT 'Added RefreshToken column.';
END
ELSE
BEGIN
    PRINT 'RefreshToken column already exists.';
END
GO

-- Add RefreshTokenExpiryTime if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RefreshTokenExpiryTime')
BEGIN
    ALTER TABLE Users ADD RefreshTokenExpiryTime DATETIME2 NULL;
    PRINT 'Added RefreshTokenExpiryTime column.';
END
ELSE
BEGIN
    PRINT 'RefreshTokenExpiryTime column already exists.';
END
GO

PRINT 'Users table update completed.';
GO
